#r "FSharp.Data.SQLProvider.dll"
#load "NaprawiaczAdresow.fsx"

open FSharp.Data.Sql

[<Literal>]
let resolutionFolder = __SOURCE_DIRECTORY__

[<Literal>]
let connectionString = "Data Source=" + __SOURCE_DIRECTORY__ + @"\pincasso.db;Version=3"

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
       a.ATR_WARTOSC <- NaprawiaczAdresow.napraw a.ATR_WARTOSC
       ctx.SubmitUpdates())
