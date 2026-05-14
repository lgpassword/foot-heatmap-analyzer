// 负责上传卡片的拖拽、文件选择反馈和弹窗联动。
export class UploadController {
    // 保存当前页面文档对象。
    document;

    // 创建上传控制器。
    constructor(document) {
        this.document = document;
    }

    // 绑定上传区域和文件输入框。
    bind() {
        const uploadZone = this.document.querySelector(".upload-zone");
        const fileInput = this.document.getElementById("ScanFile");
        if (!uploadZone || !fileInput) return;

        for (const eventName of ["dragenter", "dragover"]) {
            uploadZone.addEventListener(eventName, event => {
                event.preventDefault();
                uploadZone.classList.add("drag-over");
            });
        }

        for (const eventName of ["dragleave", "drop"]) {
            uploadZone.addEventListener(eventName, event => {
                event.preventDefault();
                uploadZone.classList.remove("drag-over");
            });
        }

        uploadZone.addEventListener("drop", event => this.handleDrop(event, uploadZone, fileInput));
        fileInput.addEventListener("change", () => this.handleFileSelected(uploadZone, fileInput));
    }

    // 处理拖拽文件并打开导入弹窗。
    handleDrop(event, uploadZone, fileInput) {
        const droppedFiles = event.dataTransfer?.files;
        if (!droppedFiles?.length) return;

        fileInput.files = droppedFiles;
        uploadZone.classList.add("file-ready");
        this.updateUploadCopy(droppedFiles[0].name);
        this.openImportModal();
    }

    // 处理普通文件选择后的 UI 反馈。
    handleFileSelected(uploadZone, fileInput) {
        const selectedFile = fileInput.files?.[0];
        if (!selectedFile) return;

        uploadZone.classList.add("file-ready");
        this.updateUploadCopy(selectedFile.name);
    }

    // 更新上传区域中显示的文件名。
    updateUploadCopy(fileName) {
        const uploadTitle = this.document.querySelector(".upload-title");
        if (uploadTitle) uploadTitle.textContent = `已选择：${fileName}`;
    }

    // 使用 Bootstrap 打开导入弹窗。
    openImportModal() {
        const modalElement = this.document.getElementById("importScanModal");
        if (!modalElement || !window.bootstrap?.Modal) return;

        window.bootstrap.Modal.getOrCreateInstance(modalElement).show();
    }
}
