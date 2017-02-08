namespace ODataClient
open Types
open Microsoft.Data.OData
open Microsoft.Data.Edm
open System.Collections.Generic
open System.IO
open System.Net
open System
module Facade=

  type ParserSettings<'TOutput,'TInput,'TMetadata>( ResponseHandler:Func<Stream,'TOutput seq>,
                                                    WebExceptionHandler:Action<WebException>,
                                                    FatalExceptionHandler:Action<Exception>,
                                                    InputEntryHandler:Func<'TInput,ODataEntry>,
                                                    MetadataHandler:Func<IEdmModel,'TMetadata seq> )=
       let getFunc (func:Func<'a,'b>)= fun x-> func.Invoke(x) 
       let getAction (func:Action<'a>)=fun x-> func.Invoke(x) 
                                                     
       member this.ResponseHandler with get()=(fun (r:HttpWebResponse)->r.GetResponseStream() |> (getFunc ResponseHandler))
       member this.WebExceptionHandler with get()=getAction WebExceptionHandler
       member this.FatalExceptionHandler with get()=getAction FatalExceptionHandler
       member this.InputEntryHandler with get()=getFunc InputEntryHandler
       member this.MetadataHandler with get()=getFunc MetadataHandler



  type IODataClient<'TOutput,'TInput,'TMetadata>=
      abstract member GetEntries:unit->seq<'TOutput>
      abstract member GetMetadata:unit->seq<'TMetadata>
      abstract member ClearMetadata:unit->unit
      abstract member WriteEntries :seq<'TInput>->unit

  type private ODataClient<'TOutput,'TInput,'TMetadata>(settings:ODataSettings,parserSettings:ParserSettings<'TOutput,'TInput,'TMetadata>)=
    
    let applyFunctors x = x parserSettings.InputEntryHandler  parserSettings.WebExceptionHandler parserSettings.FatalExceptionHandler 
    let get()=OData.Get settings |> (fun x-> x parserSettings.FatalExceptionHandler  parserSettings.WebExceptionHandler parserSettings.ResponseHandler  ) 
    let post=OData.Post settings  |> applyFunctors
    let put=OData.Put settings  |> applyFunctors
    let patch=OData.Patch settings  |> applyFunctors



    interface IODataClient<'TOutput,'TInput,'TMetadata> with
      member this.GetEntries()=match get()
                                             with
                                            |None->Seq.empty
                                            |Some l ->l

      member this.GetMetadata()=match OData.GetMetadata settings parserSettings.WebExceptionHandler parserSettings.FatalExceptionHandler  with
                                |None->Seq.empty 
                                |Some m->parserSettings.MetadataHandler(m)
                                 
      member this.ClearMetadata()=Metadata.removeFromCache settings.Uri|>ignore
      member this.WriteEntries(data)=data|>Seq.iter(fun d->post d|>ignore)

  let CreateClient<'TOutput,'TInput,'TMetadata> settings parserSettings=ODataClient(settings,parserSettings):>IODataClient<'TOutput,'TInput,'TMetadata>

