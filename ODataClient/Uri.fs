namespace ODataClient
open System
open System.Net.Http
open System.Net
open System.Web
open Builder
module Uri=
  let create path= Uri(path)
  let createRelative  (path:string) (uri:Uri) =Uri(uri,path)


  let addQueryParam<'a> name (value:'a option) (uri:Uri)=
     
      match value with
      |Some x->  let builder=UriBuilder(uri)
                 let collection=System.Web.HttpUtility.ParseQueryString(builder.Query) 
                 collection.Add(name,x.ToString())
                 builder.Query<-collection.ToString()
                 builder.Uri

      |None->uri
     

  let removeQueryParam name (uri:Uri)=
      let builder=UriBuilder(uri)
      let collection=System.Web.HttpUtility.ParseQueryString(builder.Query)
      collection.Remove(name)
      builder.Query<-collection.ToString()
      builder.Uri

  let formCollectionPart col key=
     let value=match key with
                |EntityKey.Single v->sprintf "%s" v
                |Multiple keys ->keys|> Seq.map (fun (k,v)->sprintf "%s=%s" k v) |>String.concat ","
     sprintf "%s(%s)" col value
   
  let formUri request uri=
      match request with
      |List (col,top,skip,filter)->uri
                                  |>createRelative col
                                  |>addQueryParam "$top" top
                                  |>addQueryParam "$skip" skip
                                  |>addQueryParam "$filter" filter
      |Update col->uri|>createRelative col
      |Read (col,key)->uri|>createRelative (formCollectionPart col key)                      
      |Delete (col,key)->uri|>createRelative (formCollectionPart col key) 
      |Metadata ->uri|>createRelative "$metadata"

  let requestMethod=function
        |List _ ->"GET"
        |Update _ ->"POST"
        |Read _ ->"GET"
        |Delete _ ->"DELETE"
        |Metadata _ ->"GET"

  let createWebRequest baseUri request=
      let uri=baseUri|>formUri request
      let r=WebRequest.CreateHttp(uri)
      r.Method<-requestMethod request
      r


  

