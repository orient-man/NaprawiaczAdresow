#r "FSharp.Data.SQLProvider.dll"
#load "NaprawiaczAdresow.fsx"

open FSharp.Data.Sql

[<Literal>]
let resolutionFolder = __SOURCE_DIRECTORY__

[<Literal>]
let connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.20.203)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=once)));User Id=pincasso;Password=1;"

type sql = SqlDataProvider<Common.DatabaseProviderTypes.ORACLE, connectionString, ResolutionPath=resolutionFolder, Owner = "PINCASSO">
let ctx = sql.GetDataContext()
let adresLokalu = 25m

query { 
    for atrybut in ctx.``[PINCASSO].[V_ATRYBUTY_UMOW]`` do
    join definicja in ctx.``[PINCASSO].[V_ATRYBUTY_DEFINICJI]`` on (atrybut.ATR_FK_ATD_ID = definicja.ATD_ID)
    where (definicja.ATD_TYP = adresLokalu && atrybut.ATR_WARTOSC.Contains("AdresID"))
    sortBy atrybut.ATR_ID
    select atrybut
}
|> Seq.iteri (fun idx a -> 
       printfn "%d: %M" idx a.ATR_ID
       a.ATR_WARTOSC <- NaprawiaczAdresow.napraw a.ATR_WARTOSC
       ctx.SubmitUpdates())
