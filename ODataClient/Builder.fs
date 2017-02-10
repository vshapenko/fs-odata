namespace ODataClient
module Builder=
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
  type Request=
    |List of Collection*Top option*Skip option*Filter option
    |Read of Collection*EntityKey
    |Update of Collection
    |Metadata
    |Delete of Collection*EntityKey