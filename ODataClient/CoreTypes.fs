namespace ODataClient
open System.IO
open System.Net
open System
module CoreTypes=
  let Build<'a> (builders:('a->'a) list) =
     builders|>List.fold (fun acc x->acc>>x) (fun x->x)
  
  type KeyName=string
  type KeyValue=string
   
  type EntityKey=
  |Single of KeyValue
  |Multiple of (KeyName*KeyValue) list


  type Collection= string
  type Filter= string
  type Top= uint32
  type Skip=uint32
  type Error=string
  type StatusCode=int

  type Request=
    |List of Collection*Top option*Skip option*Filter option
    |Read of Collection*EntityKey
    |Update of Collection*EntityKey
    |Metadata
    |Delete of Collection*EntityKey
    |Batch of (Collection*EntityKey) list

  type Response=
    |Success of HttpWebResponse
    |Error of WebException*StatusCode
    |Fatal of Exception
  
  type UserName=string
  type Password=string

  type  Authentication=
    |Unknown 
    |Basic of UserName*Password
