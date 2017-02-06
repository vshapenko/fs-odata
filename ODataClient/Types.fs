namespace ODataClient

open System
open Microsoft.Data.OData
open Microsoft.Data.Edm
open System.Collections.Generic
open System.Net.Http
open System.Net

module Types=
 type MetadataPropertyInfo={Id:string;PropertyName:string;PropertyType:string;IsKeyProperty:bool;IsMandatory:bool}
 type MetadataTypeInfo={Id:string;Name:string;Properties:MetadataPropertyInfo list}

 type AuthenticationType=
     |None=0
     |Basic=1

  type PayloadFormat=
     |Xml=0
     |JSON=1

 type ODataSettings={Uri:string;UserName:string;Password:string;AuthenticationType:AuthenticationType;PayloadFormat:PayloadFormat;Collection:string;PageSize:int option;Filter:string;Timeout:int option}
 
 type ClientResponseMessage (webResponse:HttpWebResponse)=
       do 
        if webResponse=null then raise (ArgumentNullException("webResponse"))

       member this.Headers 
                   with get()=webResponse.Headers.AllKeys|>Seq.map (fun h->new KeyValuePair<string,string>(h,webResponse.Headers.Get(h)))
       member this.GetStream()=webResponse.GetResponseStream()
       member this.SetHeader(headerName,headerValue)=raise (InvalidOperationException())

       member this.GetHeader(headerName)=
             match headerName with 
             |null->raise (ArgumentException())
             |_->webResponse.Headers.Get(headerName)

       member this.StatusCode with get()=int webResponse.StatusCode
                                  and set(value)=raise (InvalidOperationException())
       interface IODataResponseMessage with
           member this.Headers =this.Headers                  
           member this.GetStream()=this.GetStream()
           member this.SetHeader(headerName,headerValue)=this.SetHeader(headerName,headerValue)
           member this.GetHeader(headerName)=this.GetHeader(headerName)
           member this.StatusCode with get()=this.StatusCode
                                  and set(value)=this.StatusCode<-value

   type ClientRequestMessage(webRequest:HttpWebRequest)=
                                   
       member this.Headers 
                     with get()=webRequest.Headers.AllKeys|>Seq.map (fun h->new KeyValuePair<string,string>(h,webRequest.Headers.Get(h)))
          
       member this.Method  
                     with get()=webRequest.Method
                     and  set(value)=if String.IsNullOrWhiteSpace(value) then raise  (ArgumentException("Method must be a non-empty string.", "value"))
                                              else webRequest.Method <-value

       member this.Url 
                     with get()=webRequest.RequestUri
                     and set(value)=raise (InvalidOperationException())


       member this.GetStream()=webRequest.GetRequestStream()

       member this.GetHeader(headerName)=
             match headerName with 
             |null->raise (ArgumentException())
             |_->webRequest.Headers.Get(headerName)

       member this.SetHeader(headerName:string,headerValue)=
             match headerName.ToLowerInvariant() with
             |"accept"-> webRequest.Accept <- headerValue
             |"content-length"->webRequest.ContentLength<-int64 headerValue
             |"content-type"->webRequest.ContentType<-headerValue
             |"date"->webRequest.Date<-if(String.IsNullOrWhiteSpace(headerValue)) then DateTime.MinValue else DateTime.Parse(headerValue)
             |"host"->webRequest.Host<-headerValue
             |"if-modified-since"->webRequest.IfModifiedSince<-if(String.IsNullOrWhiteSpace(headerValue)) then DateTime.MinValue else DateTime.Parse(headerValue)
             |"referer"->webRequest.Referer<-headerValue
             |"transfer-encoding"->webRequest.TransferEncoding<-headerValue
             |"user-agent"->webRequest.UserAgent<-headerValue
             |_-> if(String.IsNullOrWhiteSpace(headerValue)) then webRequest.Headers.Remove(headerName) else webRequest.Headers.Set(headerName, headerValue)                                    

       interface IODataRequestMessage with
           member this.Headers =this.Headers    
           member this.Method  
                     with get()=this.Method
                     and  set(value)=this.Method<-value
           member this.Url 
                     with get()=this.Url
                     and set(value)=this.Url<-value
           member this.GetStream()=this.GetStream()
           member this.GetHeader(headerName)=this.GetHeader(headerName)  
           member this.SetHeader(headerName,headerValue)=this.SetHeader(headerName,headerValue)



         
    





