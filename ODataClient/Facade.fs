namespace ODataClient
open Types
open Microsoft.Data.OData
open System.Collections.Generic

module Facade=
  type IODataClient=
      abstract member GetEntries: unit->IEnumerable<IDictionary<string,obj>>
      abstract member GetMetadata:unit->IEnumerable<MetadataTypeInfo>
      abstract member ClearMetadata:unit->unit
      abstract member WriteEntries:IEnumerable<IDictionary<string,obj>>->unit

  type ODataClient(settings:ODataSettings)=
    interface IODataClient with
      member this.GetEntries()=match OData.Get settings
                                with
                                 |None->Seq.empty
                                 |Some l ->l|>Seq.ofList

      member this.GetMetadata()=match OData.GetMetadata settings with
                                |None->Seq.empty 
                                |Some m->Metadata.toTyped (Some m)
                                 
      member this.ClearMetadata()=Metadata.removeFromCache settings.Uri|>ignore
      member this.WriteEntries(data)=data|>Seq.iter(fun d->match (OData.Post settings d)
                                                            with
                                                            |_->()
                                                   )
