import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { HomeComponent } from './components/home/home.component';
import { PersonListComponent } from './components/person-list/person-list.component';
import { PersonCardComponent } from './components/person-card/person-card.component';
import { PersonFormComponent } from './components/person-form/person-form.component';

@NgModule({ 
  declarations: [
    AppComponent,
    HomeComponent,
    PersonListComponent,
    PersonCardComponent,
    PersonFormComponent
  ],
  bootstrap: [AppComponent], 
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' }
    ])
  ], 
  providers: [provideHttpClient(withInterceptorsFromDi())] 
})
export class AppModule { }
