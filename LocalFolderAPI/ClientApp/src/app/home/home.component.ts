import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FolderConfig } from '@shared/models/folderconfig.model.ts';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public folders: FolderConfig[];
  public apifolders: FolderConfig[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<FolderConfig[]>(`${baseUrl}api/folder`).subscribe(result => {
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
}
