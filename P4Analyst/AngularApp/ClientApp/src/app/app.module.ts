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
import { MatAnimatedIconComponent } from './components/mat-animated-icon/mat-animated-icon.component';
import { SnackBarTemplateComponent } from './components/snack-bar-template/snack-bar-template.component';
import { HoverClassDirective } from './directives/hover-class.directive';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { MessageService } from 'src/app/services/message.service';
import { HttpErrorHandler } from 'src/app/services/http-error-handler.service';
import { NotificationService } from './services/notification.service';
import { GraphViewComponent } from './graph-view/graph-view.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DragDropDirective } from './directives/drag-drop.directive';
import { FileUploadComponent } from './file-upload/file-upload.component';
import { EditDialogComponent } from './home/edit-dialog/edit-dialog.component';
import { MAT_SNACK_BAR_DATA } from '@angular/material';

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
  MatFormFieldModule,
} from '@angular/material';

@NgModule({
  declarations: [
    AppComponent,
    NavBarComponent,
    GraphComponent,
    HomeComponent,
    HelpComponent,
    GraphViewComponent,
    FileUploadComponent,
    MatAnimatedIconComponent,
    SnackBarTemplateComponent,
    EditDialogComponent,
    HoverClassDirective,
    DragDropDirective
  ],
  imports: [
    HttpClientModule,
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    LayoutModule,
    FormsModule,
    ReactiveFormsModule,

    // Material
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
    MatFormFieldModule,

    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'help', component: HelpComponent },
      { path: 'graphview', component: GraphViewComponent},
    ])
  ],
  schemas: [ CUSTOM_ELEMENTS_SCHEMA ],
  entryComponents: [SnackBarTemplateComponent, EditDialogComponent],
  providers: [
    HttpErrorHandler,
    MessageService,
    NotificationService,
    { provide: MAT_SNACK_BAR_DATA, useValue: {} }
  ],
  bootstrap: [AppComponent]
})

export class AppModule { }
