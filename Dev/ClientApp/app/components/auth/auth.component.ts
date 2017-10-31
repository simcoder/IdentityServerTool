import { Component, Inject} from '@angular/core';
import { Http} from '@angular/http';
import 'rxjs/Rx';


@Component({
    selector: 'auth',
    templateUrl: './auth.component.html'
})

export class AuthComponent {
    isPostOrPut = false;
    wait = false;
    selectedVerb: string = "GET";
    grantTypes: string = "Resource Owner";
    secret: string = "";
    clientId: string = "";
    authority: string = "";
    username: string = "";
    password: string = "";
    tokenResult: any;
    urlResult : any;
    url: string = "";
    scope: string = "";
    baseUrl: string;
    payload: string;
    urlHttpStatusResult: string;
    idpHttpStatusResult: string;
    getTokenOnly = false;
    isResourceOwner = true;

    constructor(private http: Http, @Inject('BASE_URL') baseUrl: string) {
        this.http = http;
        this.baseUrl = baseUrl;
    }

    onChangeVerb(newValue: string) {
        this.clearFields();
        if (newValue === "POST" || newValue === "PUT")
            this.isPostOrPut = true;
        else
            this.isPostOrPut = false;
    }
    onChangeGrantTypes(newValue: string) {
        if (newValue === "Client Credentials") {
            this.isResourceOwner = false;
        } else {
            this.isResourceOwner = true;
        }
    }
    onChangeGetTokenOnly(newValue: any) {
        this.clearFields();
    }
    clearFields() {
        this.urlHttpStatusResult = "";
        this.idpHttpStatusResult = "";
        this.tokenResult = "";
        this.urlResult = "";
    }
    submit(element: any, text: string) {
        this.clearFields();
        this.wait = true;
        element.textContent = text;
        let body;
        if (this.getTokenOnly) {
            body = {
                authority: this.authority,
                clientId: this.clientId,
                clientSecret: this.secret,
                scope: this.scope,
                getTokenOnly: this.getTokenOnly,
                grantType: this.grantTypes,
                username: this.username,
                password: this.password
            };
        } else {
            body = {
                authority: this.authority,
                clientId: this.clientId,
                clientSecret: this.secret,
                url: this.url,
                scope: this.scope,
                urlVerb: this.selectedVerb,
                urlPayload: this.payload,
                getTokenOnly: this.getTokenOnly,
                grantType: this.grantTypes,
                username: this.username,
                password: this.password
            };
        }
         
        this.http
            .post(this.baseUrl + 'api/auth', body)
            .subscribe(result => {
                this.wait = false;
                element.textContent = "Submit";
                //set idp response
                if (result.json().idpError != null) {
                    this.tokenResult =  result.json().idpError;
                } else {
                    this.tokenResult = "bearer " + result.json().token;
                }
                //set api response
                if (result.json().response != null) {
                    this.urlResult = "Data " + result.json().response;
                } 

                // set statuses 
                this.idpHttpStatusResult = result.json().idpStatus;
                this.urlHttpStatusResult = result.json().apiStatus;
            });
    }
}
