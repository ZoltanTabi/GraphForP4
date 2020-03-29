import { BrowserModule } from '@angular/platform-browser';
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LayoutModule } from '@angular/cdk/layout';
import { NavBarComponent } from './nav-bar/nav-bar.component';
import { GraphComponent } from './graph/graph.component';
import { HomeComponent } from './home/home.component';
import { HelpComponent } from './help/help.component';
import { MatAnimatedIconComponent } from './mat-animated-icon/mat-animated-icon.component';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { MessageService } from 'src/app/services/message.service';
import { HttpErrorHandler } from 'src/app/services/http-error-handler.service';
import { GraphViewComponent } from './graph-view/graph-view.component';

import {
  MatAutocompleteModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatNativeDateModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatRadioModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatStepperModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule,
} from '@angular/material';

@NgModule({
  declarations: [
    AppComponent,
    NavBarComponent,
    GraphComponent,
    HomeComponent,
    HelpComponent,
    GraphViewComponent,
    MatAnimatedIconComponent
  ],
  imports: [
    HttpClientModule,
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    LayoutModule,

    // Material
    MatToolbarModule,
    MatButtonModule,
    MatSidenavModule,
    MatSnackBarModule,
    MatIconModule,
    MatListModule,
    MatProgressSpinnerModule,
    MatTooltipModule,

    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'help', component: HelpComponent },
      { path: 'graphview', component: GraphViewComponent},
    ])
  ],
  schemas: [ CUSTOM_ELEMENTS_SCHEMA ],
  providers: [
    HttpErrorHandler,
    MessageService
  ],
  bootstrap: [AppComponent]
})

export class AppModule { }