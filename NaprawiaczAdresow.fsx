#r "FSharp.Data.SQLProvider.dll"
#r "FSharp.Data.dll"

open FSharp.Data
open FSharp.Data.Sql

[<Literal>]
let connectionString = @"Data Source=" + __SOURCE_DIRECTORY__ + @"\pincasso.db;Version=3"

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

type JsonOutput = JsonProvider<jsonSampleOutput>

let naprawAdres (input : JsonInput.Root) = 
    JsonOutput.Root
        (input.AdresId.ToString(), JsonOutput.Miejscowosc(input.Lokalizacja.Miejscowosc.JsonValue), 
         input.Lokalizacja.NrDomu, input.Lokalizacja.NrDzialki, input.Lokalizacja.NrMieszkania, 
         JsonOutput.Miejscowosc(input.Lokalizacja.Poczta.JsonValue), input.Lokalizacja.Skrytka, 
         JsonOutput.Miejscowosc(input.Lokalizacja.Ulica.JsonValue), input.Lokalizacja.UwagiDodatkowe)

type sql = SqlDataProvider<connectionString, Common.DatabaseProviderTypes.SQLITE, resolutionFolder>

let ctx = sql.GetDataContext()
let AdresLokalu = int64 (25)

query { 
    for atrybut in ctx.``[main].[v_atrybuty_umow]`` do
        for definicja in atrybut.FK_v_atrybuty_umow_0_0 do
            where (definicja.atd_typ = AdresLokalu && atrybut.atr_wartosc.Contains("AdresID"))
            sortBy atrybut.atr_id
            select atrybut.atr_id
}
|> Seq.toList
|> List.iteri (fun idx id -> 
       printfn "%d: %d" idx id
       let a = 
           query { 
               for atrybuty in ctx.``[main].[v_atrybuty_umow]`` do
                   where (atrybuty.atr_id = id)
                   select atrybuty
                   exactlyOne
           }
       
       let poprawiony = 
           a.atr_wartosc
           |> JsonInput.Parse
           |> naprawAdres
       
       a.atr_wartosc <- poprawiony.JsonValue.ToString()
       ctx.SubmitUpdates())
