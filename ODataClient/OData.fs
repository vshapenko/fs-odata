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

 let getEntitySet (metadata:IEdmModel) collection=
           metadata.EntityContainers()
               |>Seq.map (fun x->x.EntitySets())
               |>Seq.concat
               |>Seq.find(fun x->x.Name=collection)
     
 let readEntries (response:HttpWebResponse)=
    let message= ClientResponseMessage(response) :>IODataResponseMessage
    let reader=new ODataMessageReader(message)
    let entryReader=reader.CreateODataFeedReader()
    let res=seq{
                while (entryReader.State<>ODataReaderState.Completed && entryReader.Read()) do 
                     if(entryReader.State=ODataReaderState.EntryEnd) then yield createDictionaryItem (entryReader.Item:?>ODataEntry)
    }
    res|>List.ofSeq

 let writeDataToRequest (data:IDictionary<string,obj>) entitySet (request:HttpWebRequest)=
    let entry=createOdataItem data
    let message= ClientRequestMessage(request):>IODataRequestMessage
    let writer=new ODataMessageWriter(message)
    let entryWriter=writer.CreateODataEntryWriter()
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

    
 let createRequestFromSettings requestFunc settings=
     settings.Uri
     |>Uri.create
     |>Uri.createRelative settings.Collection
     |>requestFunc
     |>HttpRequest.build [fillPayload settings;fillAuthType settings] 

 let private processDataEntry requestFunc (settings:ODataSettings) (data:IDictionary<string,obj>) metadata=
    settings
    |>createRequestFromSettings requestFunc
    |>writeDataToRequest  data (getEntitySet metadata settings.Collection)
    |>HttpRequest.sendDefault(fun x->())

 let Get (settings:ODataSettings)=
    settings
    |>createRequestFromSettings HttpRequest.Get
    |>HttpRequest.sendDefault readEntries

 let Post settings data metadata=processDataEntry HttpRequest.Post settings data metadata
 let Put settings data metadata=processDataEntry HttpRequest.Put settings data metadata
 let Patch settings data metadata=processDataEntry HttpRequest.Patch metadata

 let Metadata (settings:ODataSettings) = Metadata.get settings.Uri (fillAuthType settings>>fillPayload settings) 

    



   