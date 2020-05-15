import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { HelpComponent } from './help/help.component';
import { GraphViewComponent } from './graph-view/graph-view.component';
import { AnalyzeComponent } from './analyze/analyze.component';
import { FileListComponent } from './file-list/file-list.component';

const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'help', component: HelpComponent },
  { path: 'graphview', component: GraphViewComponent},
  { path: 'analyze', component: AnalyzeComponent},
  { path: 'files', component: FileListComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
