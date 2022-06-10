import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { CUSTOM_ELEMENTS_SCHEMA, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgBusyModule } from 'ng-busy';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ConfigModule } from './config/config.module';
import { Configuration } from './api';
import { SharedModule, } from './shared/shared.module';
import { JwtHelperService, JWT_OPTIONS } from '@auth0/angular-jwt';
import { TranslateModule, TranslateLoader, TranslateService } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { LandingComponent } from './components/landing/landing.component';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';

import localeEn from '@angular/common/locales/en';
import localeFr from '@angular/common/locales/fr';
import { registerLocaleData } from '@angular/common';
import { CoreModule } from './core/core.module';

import { STEPPER_GLOBAL_OPTIONS } from '@angular/cdk/stepper';
import { CdkAccordionModule } from '@angular/cdk/accordion';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { TicketPageComponent } from '@components/ticket-page/ticket-page.component';
import { UnauthorizedComponent } from '@components/error/unauthorized/unauthorized.component';
import { TicketInfoComponent } from '@components/ticket-info/ticket-info.component';
import { MockConfigService } from 'tests/mocks/mock-config.service';
import { AuthInterceptor, OidcSecurityService } from 'angular-auth-oidc-client';
import { ContactInfoComponent } from './components/contact-info/contact-info.component';
import { AuthConfigModule } from './auth/auth-config.module';
import { TicketStatusComponent } from './components/ticket-status/ticket-status.component';
import { TicketRequestComponent } from '@components/ticket-request/ticket-request.component';
import { JjWorkbenchDashboardComponent } from '@components/jj-workbench-dashboard/jj-workbench-dashboard.component';

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
    TicketInfoComponent,
    ContactInfoComponent,
    TicketStatusComponent,
    TicketRequestComponent,
    JjWorkbenchDashboardComponent,
  ],
  imports: [
    CommonModule,
    BrowserModule,
    AppRoutingModule,
    CoreModule,
    SharedModule,
    ConfigModule,
    HttpClientModule,
    AuthConfigModule,
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
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  exports: [NgBusyModule, TranslateModule],
  providers: [
    CurrencyPipe,
    DatePipe,
    MockConfigService,
    JwtHelperService,
    { provide: JWT_OPTIONS, useValue: JWT_OPTIONS }, 
    {
      provide: STEPPER_GLOBAL_OPTIONS,
      useValue: { showError: true }
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    {
      provide: Configuration, // this configuration together with oidc configuration parameter secureroutes adds the bearer token to /api calls
      useFactory: (authService: OidcSecurityService) => new Configuration(
        {
          basePath: '',//environment.apiUrl,
          accessToken: authService.getAccessToken.bind(authService),
          credentials: {
            'Bearer': () => {
              var token: string = authService.getAccessToken.bind(authService);
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
  bootstrap: [AppComponent]
})
export class AppModule {
  private availableLanguages = ['en', 'fr'];

  constructor(private translateService: TranslateService) {
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
