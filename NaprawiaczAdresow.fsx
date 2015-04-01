#r "FSharp.Data.dll"
open FSharp.Data

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

let napraw input = 
    input
    |> JsonInput.Parse
    |> fun input -> 
        let lok = input.Lokalizacja
        JsonOutput.Lokalizacja
            (adres = lok.Adres, miejscowosc = KVP(lok.Miejscowosc.JsonValue), nrDomu = lok.NrDomu, 
             nrDzialki = lok.NrDzialki, nrMieszkania = lok.NrMieszkania, 
             poczta = KVP(lok.Poczta.JsonValue), skrytka = lok.Skrytka, 
             ulica = KVP(lok.Ulica.JsonValue), uwagiDodatkowe = lok.UwagiDodatkowe)
    |> fun output -> output.JsonValue.ToString()
