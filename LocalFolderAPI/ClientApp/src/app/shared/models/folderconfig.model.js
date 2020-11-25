"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.FolderConfig = void 0;
var FolderConfig = /** @class */ (function () {
    function FolderConfig(object) {
        this.folderName = (object && object.folderName) ? object.folderName : null;
        this.path = (object && object.path) ? object.path : null;
        this.pollingType = (object && object.pollingType) ? object.pollingType : null;
        this.polling = (object && object.polling) ? object.polling : null;
        this.moveToFolder = (object && object.moveToFolder) ? object.moveToFolder : null;
        this.apiUrl = (object && object.apiUrl) ? object.apiUrl : null;
    }
    return FolderConfig;
}());
exports.FolderConfig = FolderConfig;
//# sourceMappingURL=folderconfig.model.js.map