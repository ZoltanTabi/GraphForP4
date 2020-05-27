import { Component, ViewChild, OnInit } from '@angular/core';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginatorIntl } from '@angular/material';
import { FileService } from '../services/file.service';
import { SessionStorageService, LocalStorageService } from 'ngx-store';
import { FileData } from '../models/file';
import { NotificationService } from '../services/notification.service';
import { ClipboardService } from 'ngx-clipboard';
import { HomeService } from '../home/home.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-file-list',
  templateUrl: './file-list.component.html',
  providers: [HomeService],
  styleUrls: ['./file-list.component.scss']
})
export class FileListComponent implements OnInit {

  displayedColumns: string[] = ['name', 'createDate', 'copy', 'runGraph'];
  dataSource: MatTableDataSource<FileData>;
  formatDate = 'yyyy-MM-dd';

  @ViewChild(MatPaginator, {static: true}) paginator: MatPaginator;
  @ViewChild(MatSort, {static: true}) sort: MatSort;

  // tslint:disable-next-line:max-line-length
  constructor(private matPaginatorIntl: MatPaginatorIntl, private fileService: FileService, private localStorageService: LocalStorageService,
    private notificationService: NotificationService, private clipboardService: ClipboardService, private homeService: HomeService,
    private router: Router, private sessionStorageService: SessionStorageService) { }

  ngOnInit() {
    this.fileService.getFiles().subscribe((result) => {
      if (result.length === 0) {
        this.notificationService.info('Nincsen megjelenetíthető fájl!');
      }
      this.dataSource = new MatTableDataSource(result);
      this.matPaginatorIntl.nextPageLabel = 'Következő oldal';
      this.matPaginatorIntl.previousPageLabel = 'Előző oldal';
      this.matPaginatorIntl.itemsPerPageLabel = 'elem / oldal';
      this.dataSource.paginator = this.paginator;
      this.dataSource.sort = this.sort;
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  copy(id: number) {
    const file = this.getFileData(id);
    this.clipboardService.copyFromContent(file.content);
    this.notificationService.success('Sikeres másolás!');
  }

  runGraph(id: number) {
    const file = this.getFileData(id);
    this.homeService
      .sendFileContent(file)
      .subscribe(() => {
        this.sessionStorageService.clear('prefix');
        this.router.navigateByUrl('/graphview');
      });
  }

  getFileData(id: number): FileData {
    const file = this.localStorageService.get(id.toString()) as FileData;
    if (file && file.content && file.content.length > 0) {
      return file;
    } else {
      this.fileService.getFile(id).subscribe((result) => {
        this.localStorageService.set(id.toString(), result);
        return result;
      });
    }
  }
}
