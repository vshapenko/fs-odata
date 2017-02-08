namespace ODataClient
open System
open System.Net
open System.Net.Http
open System.Text
open Microsoft.Data.OData
open Microsoft.Data.Edm
open System.Collections.Generic
open Types
module OData=
 
 let createOdataItem (data:IDictionary<string,obj>)=
      let entry=new ODataEntry()
      let properties=data|>Seq.map(fun x-> let property= new ODataProperty()
                                           property.Name<-x.Key
                                           property.Value<-x.Value
                                           property
                                   )|>Seq.toList
      entry.Properties<-properties
      entry

 let createDictionaryItem(item:ODataEntry)=
     item.Properties|>Seq.map (fun p-> (p.Name,p.Value))|>dict

 let getEntitySet (metadata:IEdmModel option) collection=
           match metadata with
           |Some x->x.EntityContainers()
                   |>Seq.map (fun x->x.EntitySets())
                   |>Seq.concat
                   |>Seq.tryFind(fun x->x.Name=collection)
           |None->None

     
 let readEntries (response:HttpWebResponse)=
    let message= ClientResponseMessage(response) :>IODataResponseMessage
    let reader=new ODataMessageReader(message)
    let entryReader=reader.CreateODataFeedReader()
    let res=seq{
                while (entryReader.State<>ODataReaderState.Completed && entryReader.Read()) do 
                     if(entryReader.State=ODataReaderState.EntryEnd) then yield createDictionaryItem (entryReader.Item:?>ODataEntry)
    }
    res|>List.ofSeq

 let writeDataToRequest (dataFunc:'a->ODataEntry) entitySet data (request:HttpWebRequest) =
    let entry=dataFunc data
    let message= ClientRequestMessage(request):>IODataRequestMessage
    let writer=ODataMessageWriter(message)
    let entryWriter= match entitySet with
                       |Some x->writer.CreateODataEntryWriter(x)
                       |None->writer.CreateODataEntryWriter()
    entryWriter.WriteStart(entry)
    entryWriter.WriteEnd()
    request

 let fillAuthType settings =
      match settings.AuthenticationType with
      |AuthenticationType.Basic->HttpRequest.addBasicAuth settings.UserName settings.Password 
      |_->fun r->r

 let fillPayload settings =
      match settings.PayloadFormat with
     |PayloadFormat.JSON->HttpRequest.acceptJson 
     |PayloadFormat.Xml->HttpRequest.acceptXml 
     |_->fun r->r

 let fillUriParams (settings:ODataSettings) uri=
     seq{
         if(not (String.IsNullOrWhiteSpace(settings.Filter))) then yield Uri.addQueryParam "filter" settings.Filter
         match settings.PageSize with
          |Some x->yield Uri.addQueryParam "top" (x.ToString())
          |None->()
     }
    
 let createRequestFromSettings requestFunc  settings=
     settings.Uri
     |>Uri.create
     |>Uri.createRelative settings.Collection
     |>requestFunc
     |>HttpRequest.build [fillPayload settings;fillAuthType settings] 

 let GetMetadata (settings:ODataSettings) =  match Metadata.Cache.TryGetValue(settings.Uri) with
                                             |true,x->x
                                             |false,_->Metadata.get settings.Uri (fillAuthType settings>>HttpRequest.acceptXml)
                                                       |>Metadata.addToCache settings.Uri
  
  
 let private processDataEntry requestFunc (settings:ODataSettings) dataFunc error fatal  data=
    let metadata=GetMetadata settings
    settings
    |>createRequestFromSettings requestFunc
    |>writeDataToRequest dataFunc (getEntitySet metadata settings.Collection)  data 
    |>HttpRequest.Send
    |>fun x->(x fatal error (fun r->()))
 
 let Get (settings:ODataSettings) =
    settings
    |>createRequestFromSettings HttpRequest.Get
    |>HttpRequest.Send

 let Post settings =processDataEntry HttpRequest.Post settings  
 let Put settings  =processDataEntry HttpRequest.Put settings   
 let Patch settings  =processDataEntry HttpRequest.Patch settings 



    



   