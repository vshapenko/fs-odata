namespace ODataClient
open Types
open ODataRequests
module Facade=

  type ODataClient(settings:ODataSettings)=
    let mutable metadata=null
    interface IODataClient with
      member this.GetEntries()=match getData settings
                                with
                                 |None->Seq.empty
                                 |Some l ->l|>Seq.ofList

      member this.GetMetadata()=match getMetadata settings with
                                |None->null 
                                |Some m ->
                                 metadata<-m
                                 m
      member this.ClearMetadata()=metadata<-null
      member this.WriteEntries(data)=data|>Seq.iter(fun d->match (processDataEntry postRequest settings d metadata) 
                                                            with
                                                            |_->()
                                                   )
