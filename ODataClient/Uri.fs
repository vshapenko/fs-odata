namespace ODataClient
open System

module Uri=
  let create path= Uri(path)
  let createRelative  (path:string) (uri:Uri) =Uri(uri,path)

