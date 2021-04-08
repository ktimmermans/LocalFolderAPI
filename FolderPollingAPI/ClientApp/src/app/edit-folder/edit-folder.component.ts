import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FolderConfig } from '@shared/models/folderconfig.model.ts';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-edit-folder',
  templateUrl: './edit-folder.component.html',
})
export class EditFolderComponent {
  public newfolder: FolderConfig;
  public currentfolder: FolderConfig;
  public currentfoldername: string;
  public subfolders: string[];
  public subFolderError: string;
  public submitError: string;

  private httpClient: HttpClient;
  private baseUrl: string;

  constructor(
    http: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    private activatedRoute: ActivatedRoute,
    private router: Router) {
    this.httpClient = http;
    this.newfolder = new FolderConfig();
    this.baseUrl = baseUrl;
  }

  public ngOnInit(): void {
    this.getCurrentFolder();
  }

  public getCurrentFolder(): void {

    this.activatedRoute.queryParams.subscribe(params => {
      this.currentfoldername= params['folderName'];
    })

    this.httpClient
      .get<FolderConfig>(`${this.baseUrl}api/v1/folder/${encodeURI(this.currentfoldername)}/settings`)
      .subscribe(result => {
        this.currentfolder = result;
        this.setCurrentToNewFolder();
    }, error => {
      console.error(`${error.error.error}`);
    });
  }

  public setCurrentToNewFolder(): void {

    // Check if current has API url otherwise set default
    if (!this.currentfolder.apiUrl) { 
      this.currentfolder.apiUrl = `${this.baseUrl}api/v1/test/receive`;
    }

    this.newfolder = this.currentfolder;
  }

  public getSubfolders(): void {
    this.httpClient.get<string[]>(`${this.baseUrl}api/v1/folder/${encodeURI(this.currentfolder.path)}`).subscribe(result => {
      this.subFolderError = null;
      this.subfolders = result;
    }, error => {
      console.error(`${error.error.error}`);
      this.subFolderError = error.error.error;
    });
  }

  public setPath(path: string): void {
    this.currentfolder.path = path;
    this.getSubfolders();
  }

  public updateFolder(oldFolder): void {
    this.httpClient.post(`${this.baseUrl}api/v1/folder/${oldFolder}/update`, this.newfolder).subscribe(result => {
      this.router.navigate(['/']);
    }, error => {
      console.error(`${error}`);
      this.submitError = error.message;
    })
  }
}
