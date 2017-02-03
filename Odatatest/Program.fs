// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open Simple.OData.Client
open System
open System.Text
open System.Threading.Tasks
open Microsoft.Data.Edm
open Microsoft.FSharp.Data.TypeProviders
open ODataClient.Types
open ODataClient.Facade

[<EntryPoint>]
let main argv =  
    let settings={Uri="http://services.odata.org/V2/(S(tuach5452uhyzt5qkoqbhmzc))/OData/OData.svc/";UserName="";Password="";AuthenticationType=AuthenticationType.None;PayloadFormat=PayloadFormat.Xml;Collection="Products";PageSize=None;Filter=null;Timeout=None}
    let client=ODataClient(settings):>IODataClient
    let data=client.GetEntries()
    printfn "%A" argv
    0 // return an integer exit code

