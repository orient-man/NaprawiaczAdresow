#r "FSharp.Data.SQLProvider.dll"
#r "FSharp.Data.dll"

open FSharp.Data
open FSharp.Data.Sql

[<Literal>]
let connectionString = "Data Source=" + __SOURCE_DIRECTORY__ + @"\pincasso.db;Version=3"

[<Literal>]
let resolutionFolder = __SOURCE_DIRECTORY__

[<Literal>]
let jsonSampleInput = """{
    "AdresID":186875,
    "Lokalizacja":{
        "Adres":"Wrocławska 25, 56-400 Oleśnica",
        "ID":84915,
        "Miejscowosc":{"key":30265,"value":"Oleśnica"},
        "NrDomu":"teskt",
        "NrDzialki":"tekst",
        "NrMieszkania":"tekst",
        "Poczta":{"key":21186,"value":"56-400"},
        "Skrytka":"tekst",
        "Ulica":{"key":34645,"value":"Wrocławska"},
        "UwagiDodatkowe":"tekst"}
}"""

[<Literal>]
let jsonSampleOutput = """{
    "Adres":"Wrocławska 25, 56-400 Oleśnica",
    "Miejscowosc":{"key":30265,"value":"Oleśnica"},
    "NrDomu":"napis",
    "NrDzialki":"tekst",
    "NrMieszkania":"tekst",
    "Poczta":{"key":21186,"value":"56-400"},
    "Skrytka":"tekst",
    "Ulica":{"key":34645,"value":"Wrocławska"},
    "UwagiDodatkowe":"tekst"
}"""

type JsonInput = JsonProvider<jsonSampleInput>

type JsonOutput = JsonProvider<jsonSampleOutput, RootName="Lokalizacja">

type KVP = JsonOutput.Miejscowosc

let naprawAdres input = 
    input
    |> JsonInput.Parse
    |> fun input -> 
        let lok = input.Lokalizacja
        JsonOutput.Lokalizacja
            (lok.Adres, KVP(lok.Miejscowosc.JsonValue), lok.NrDomu, lok.NrDzialki, lok.NrMieszkania, 
             KVP(lok.Poczta.JsonValue), lok.Skrytka, KVP(lok.Ulica.JsonValue), lok.UwagiDodatkowe)

type sql = SqlDataProvider<Common.DatabaseProviderTypes.SQLITE, connectionString, ResolutionPath=resolutionFolder>

let ctx = sql.GetDataContext()
let adresLokalu = 25L

query { 
    for atrybut in ctx.``[MAIN].[V_ATRYBUTY_UMOW]`` do
        for definicja in atrybut.FK_v_atrybuty_umow_0_0 do
            where (definicja.ATD_TYP = adresLokalu && atrybut.ATR_WARTOSC.Contains("AdresID"))
            sortBy atrybut.ATR_ID
            select atrybut
}
|> Seq.iteri (fun idx a -> 
       printfn "%d: %d" idx a.ATR_ID
       a.ATR_WARTOSC <- (naprawAdres a.ATR_WARTOSC).JsonValue.ToString()
       ctx.SubmitUpdates())
