﻿namespace ODataClient
open Microsoft.Data.OData
open Microsoft.Data.Edm
open System.Net
open System.Collections.Generic
open Types
module Metadata=
  let Cache=new Dictionary<string,IEdmModel>()
  let private readMetadataRaw (response:HttpWebResponse)=
       let message= ClientResponseMessage(response) :>IODataResponseMessage
       let reader=new ODataMessageReader(message)
       reader.ReadMetadataDocument()

  let private createMeadataProperty (keys:Set<string>) (p:IEdmProperty)=
       {Id=p.Name;PropertyName=p.Name;PropertyType=p.Type.PrimitiveKind().ToString();IsKeyProperty=keys.Contains(p.Name);IsMandatory=p.Type.IsNullable}

  let private createMetadataType (entitySet:IEdmEntitySet)=
       let entityType=entitySet.ElementType
       let keys=entityType.DeclaredKey|>Seq.map (fun x->x.Name)|>Set.ofSeq
       let properties=entityType.Properties()
                      |>Seq.map (createMeadataProperty keys)
                      |>List.ofSeq
       {Id=entityType.Name;Properties=properties;Name=entityType.Name}
    
  let toTyped (metadata:IEdmModel)=
       metadata.EntityContainers()
       |>Seq.map (fun x->x.EntitySets())
       |>Seq.concat
       |>Seq.map createMetadataType

  let get url requestBuilder=
    url
    |> Uri.create
    |> HttpRequest.Get
    |> requestBuilder
    |> HttpRequest.sendDefault readMetadataRaw 
  
  let addToCache url metadata=Cache.Add(url,metadata)
  let removeFromCache url =Cache.Remove(url)
