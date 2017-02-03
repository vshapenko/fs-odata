namespace ODataClient
open System
open System.Net.Http
open System.Web
module Uri=
  let create path= Uri(path)
  let createRelative  (path:string) (uri:Uri) =Uri(uri,path)


  let addQueryParam name value (uri:Uri)=
      let builder=UriBuilder(uri)
      let collection=System.Web.HttpUtility.ParseQueryString(builder.Query)
      collection.Add(name,value)
      builder.Query<-collection.ToString()
      builder.Uri

  let removeQueryParam name (uri:Uri)=
      let builder=UriBuilder(uri)
      let collection=System.Web.HttpUtility.ParseQueryString(builder.Query)
      collection.Remove(name)
      builder.Query<-collection.ToString()
      builder.Uri

