import { BrowserModule } from '@angular/platform-browser';
import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LayoutModule } from '@angular/cdk/layout';
import { DragDropModule } from '@angular/cdk/drag-drop';
import { NavBarComponent } from './nav-bar/nav-bar.component';
import { GraphComponent } from './graph/graph.component';
import { HomeComponent } from './home/home.component';
import { HelpComponent } from './help/help.component';
import { AnalyzeComponent } from './analyze/analyze.component';
import { SelectorComponent } from './selector/selector.component';
import { DragAndDropListComponent } from './drag-and-drop-list/drag-and-drop-list.component';
import { FileListComponent } from './file-list/file-list.component';
import { ChartsDisplayComponent } from './charts-display/charts-display.component';
import { GraphCarouselComponent } from './components/graph-carousel/graph-carousel.component';
import { MatAnimatedIconComponent } from './components/mat-animated-icon/mat-animated-icon.component';
import { SnackBarTemplateComponent } from './components/snack-bar-template/snack-bar-template.component';
import { BottomSheetTemplateComponent } from '../app/components/bottom-sheet-template/bottom-sheet-template.component';
import { BottomSheetYesOrNoComponent } from '../app/components/bottom-sheet-yes-or-no/bottom-sheet-yes-or-no.component';
import { HoverClassDirective } from './directives/hover-class.directive';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { MessageService } from 'src/app/services/message.service';
import { HttpErrorHandler } from 'src/app/services/http-error-handler.service';
import { NotificationService } from './services/notification.service';
import { FileService } from './services/file.service';
import { GraphViewComponent } from './graph-view/graph-view.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DragDropDirective } from './directives/drag-drop.directive';
import { FileUploadComponent } from './file-upload/file-upload.component';
import { EditDialogComponent } from './file-upload/edit-dialog/edit-dialog.component';
import { AngularResizedEventModule } from 'angular-resize-event';
import { WebStorageModule } from 'ngx-store';
import { ClipboardModule } from 'ngx-clipboard';
import { ChartsModule } from 'ng2-charts';

import {
  MatAutocompleteModule,
  MatBottomSheetModule,
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
  MatPaginatorModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatRadioModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatSortModule,
  MatStepperModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule,
  MatFormFieldModule,
  MAT_SNACK_BAR_DATA,
  MAT_BOTTOM_SHEET_DATA
} from '@angular/material';

import { OverlayModule } from '@angular/cdk/overlay';

@NgModule({
  declarations: [
    AppComponent,
    NavBarComponent,
    GraphComponent,
    HomeComponent,
    HelpComponent,
    AnalyzeComponent,
    SelectorComponent,
    DragAndDropListComponent,
    ChartsDisplayComponent,
    GraphCarouselComponent,
    FileListComponent,
    GraphViewComponent,
    FileUploadComponent,
    MatAnimatedIconComponent,
    SnackBarTemplateComponent,
    BottomSheetTemplateComponent,
    BottomSheetYesOrNoComponent,
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
    AngularResizedEventModule,
    OverlayModule,
    WebStorageModule,
    ClipboardModule,
    ChartsModule,

    // Material
    DragDropModule,
    MatAutocompleteModule,
    MatBottomSheetModule,
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
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
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
      { path: 'analyze', component: AnalyzeComponent},
      { path: 'files', component: FileListComponent}
    ])
  ],
  schemas: [ CUSTOM_ELEMENTS_SCHEMA ],
  entryComponents: [SnackBarTemplateComponent, EditDialogComponent, BottomSheetTemplateComponent, BottomSheetYesOrNoComponent],
  providers: [
    HttpErrorHandler,
    MessageService,
    NotificationService,
    FileService,
    { provide: MAT_SNACK_BAR_DATA, useValue: {} },
    { provide: MAT_BOTTOM_SHEET_DATA, useValue: {} }
  ],
  bootstrap: [AppComponent]
})

export class AppModule { }
