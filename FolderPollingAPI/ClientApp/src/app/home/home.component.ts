import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FolderConfig } from '@shared/models/folderconfig.model.ts';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public folders: FolderConfig[];
  public apifolders: FolderConfig[];
  httpClient: HttpClient;
  private baseUrl: string;
  public submitError: string;

  constructor(
    http: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    private router: Router
  ) {
    this.httpClient = http;
    this.baseUrl = baseUrl;
    http.get<FolderConfig[]>(`${baseUrl}api/v1/folder`).subscribe(result => {
      this.folders = result;
    }, error => console.error(error));
  }

  public get apiFolders(): FolderConfig[] {
    if (this.folders) {
      return this.folders.filter(function (folder: FolderConfig) {
        return !folder.polling;
      });
    }
  }

  public deleteFolder(folderName): void {

    if (confirm("Are you sure you wish to delete " + folderName)) {
      this.httpClient.post(`${this.baseUrl}api/v1/folder/${folderName}/delete`, this.deleteFolder).subscribe(result => {
        location.reload();
      }, error => {
        console.error(`${error}`);
        this.submitError = error.message;
      })
    }
  }

}
