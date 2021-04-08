import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FolderConfig } from '@shared/models/folderconfig.model.ts';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-folder',
  templateUrl: './add-folder.component.html',
})
export class AddFolderComponent {
  public newfolder: FolderConfig;
  public subfolders: string[];
  public subFolderError: string;
  public submitError: string;

  private httpClient: HttpClient;
  private baseUrl: string;

  constructor(
    http: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    private router: Router) {
    this.httpClient = http;
    this.newfolder = new FolderConfig();
    this.newfolder.polling = false;
    this.baseUrl = baseUrl;
    this.newfolder.apiUrl = `${this.baseUrl}api/v1/test/receive`;
  }

  public getSubfolders(): void {
    this.httpClient.get<string[]>(`${this.baseUrl}api/v1/folder/${encodeURI(this.newfolder.path)}`).subscribe(result => {
      this.subFolderError = null;
      this.subfolders = result;
    }, error => {
      console.error(`${error.error.error}`);
      this.subFolderError = error.error.error;
    });
  }

  public setPath(path: string): void {
    this.newfolder.path = path;
    this.getSubfolders();
  }

  public addFolder(): void {
    this.httpClient.post(`${this.baseUrl}api/v1/folder`, this.newfolder).subscribe(result => {
      this.newfolder = new FolderConfig();
      this.router.navigate(['/']);
    }, error => {
      console.error(`${error}`);
      this.submitError = error.message;
    })
  }

}
