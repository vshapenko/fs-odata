namespace ODataClient
open Types
module Facade=

  type ODataClient(settings:ODataSettings)=
    let mutable metadata=null
    interface IODataClient with
      member this.GetEntries()=match OData.Get settings
                                with
                                 |None->Seq.empty
                                 |Some l ->l|>Seq.ofList

      member this.GetMetadata()=match OData.Metadata settings with
                                |None->null 
                                |Some m ->
                                 metadata<-m
                                 m
      member this.ClearMetadata()=metadata<-null
      member this.WriteEntries(data)=data|>Seq.iter(fun d->match OData.Post settings d metadata 
                                                            with
                                                            |_->()
                                                   )
