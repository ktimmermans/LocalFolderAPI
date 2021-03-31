export class FolderConfig {
  public folderName: string;
  public path: string;
  public pollingType: string;
  public polling: boolean;
  public moveToFolder: string;
  public apiUrl: string;
  public isRecursive: boolean;
  public canOverwriteFiles: boolean;

  constructor(object?: FolderConfig) {
    this.folderName = (object && object.folderName) ? object.folderName : null;
    this.path = (object && object.path) ? object.path : null;
    this.pollingType = (object && object.pollingType) ? object.pollingType : null;
    this.polling = (object && object.polling) ? object.polling : null;
    this.moveToFolder = (object && object.moveToFolder) ? object.moveToFolder : null;
    this.apiUrl = (object && object.apiUrl) ? object.apiUrl : null;
    this.isRecursive = (object && object.isRecursive) ? object.isRecursive : false;
    this.canOverwriteFiles = (object && object.canOverwriteFiles) ? object.canOverwriteFiles : false;
  }
}
