import {
  HttpClient,
  HttpClientModule,
  HTTP_INTERCEPTORS,
} from '@angular/common/http';
import { CUSTOM_ELEMENTS_SCHEMA, NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
// import { BackendHttpInterceptor } from '@core/interceptors/backend-http.interceptor';
import { NgBusyModule } from 'ng-busy';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ConfigModule } from './config/config.module';
import { Configuration } from './api/configuration';
// import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import {
  TranslateModule,
  TranslateLoader,
  TranslateService,
} from '@ngx-translate/core';
import { Observable } from "rxjs";
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { LandingComponent } from './components/landing/landing.component';
import { MatStepperModule } from '@angular/material/stepper';
import { MatSortModule } from '@angular/material/sort';
import { MatCheckboxModule } from '@angular/material/checkbox'
import { MatIconModule } from '@angular/material/icon';
import { CommonModule, CurrencyPipe } from '@angular/common';

import localeEn from '@angular/common/locales/en';
import localeFr from '@angular/common/locales/fr';
import { registerLocaleData } from '@angular/common';
import { CoreModule } from './core/core.module';

import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { CdkAccordionModule} from '@angular/cdk/accordion';
import {NgxMaterialTimepickerModule} from 'ngx-material-timepicker';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TicketPageComponent } from '@components/ticket-page/ticket-page.component';
import { UnauthorizedComponent } from '@components/error/unauthorized/unauthorized.component';

import { DateSuffixPipe } from './services/date.service';
import { InterceptorService } from './services/interceptor.service';
import { TicketInfoComponent } from '@components/ticket-info/ticket-info.component';
import { OidcSecurityService, EventTypes, PublicEventsService, AuthModule, LogLevel } from 'angular-auth-oidc-client';

registerLocaleData(localeEn, 'en');
registerLocaleData(localeFr, 'fr');

// export function appInit(appConfigService: AppConfigService) {
//   return () => {
//     return appConfigService.loadAppConfig();
//   };
// }

export function HttpLoaderFactory(http: HttpClient): TranslateHttpLoader {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

@NgModule({
  declarations: [
    AppComponent,
    LandingComponent,
    TicketPageComponent,
    UnauthorizedComponent,
    DateSuffixPipe,
    TicketInfoComponent
  ],
  imports: [
    CommonModule,
    BrowserModule,
    AppRoutingModule,
    CoreModule,
    SharedModule,
    ConfigModule,
    HttpClientModule,
    MatStepperModule,
    MatSortModule,
    MatIconModule,
    MatCheckboxModule,
    CdkAccordionModule,
    BrowserAnimationsModule,
    NgxMaterialTimepickerModule,
    FormsModule,
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient],
      },
      isolate: false,
      extend: true,
    }),
  ],
  schemas: [ CUSTOM_ELEMENTS_SCHEMA ],
  exports: [NgBusyModule, TranslateModule],
  providers: [
    CurrencyPipe,
    // AppConfigService,
    // {
    //   provide: HTTP_INTERCEPTORS,
    //   useClass: BackendHttpInterceptor,
    //   multi: true,
    // },
    {
      provide: STEPPER_GLOBAL_OPTIONS,
      useValue: { showError: true }
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: InterceptorService,
      multi: true
    },
    {
      provide: Configuration,
      useFactory: (authService: OidcSecurityService) => new Configuration(
        {
          basePath: '',//environment.apiUrl,
          accessToken: authService.getAccessToken.bind(authService),
          credentials: {
            'Bearer': () => {
              var token: any = authService.getAccessToken();
              if (token) {
                return 'Bearer ' + token;
              }
              return undefined;
            }
          }
        }
      ),
      deps: [OidcSecurityService],
      multi: false
    },
  ],
  bootstrap: [AppComponent],
  
})
export class AppModule {
  private availableLanguages = ['en', 'fr'];

  constructor(private translateService: TranslateService, private oidcSecurityService: OidcSecurityService) {
    this.translateService.addLangs(['en', 'fr']);

    const currentLanguage = window.navigator.language.substring(0, 2);
    // console.log('Current Browser Language', currentLanguage);

    let defaultLanguage = 'en';
    if (this.availableLanguages.includes(currentLanguage)) {
      defaultLanguage = currentLanguage;
    }
    this.translateService.setDefaultLang(defaultLanguage);
  }
}
