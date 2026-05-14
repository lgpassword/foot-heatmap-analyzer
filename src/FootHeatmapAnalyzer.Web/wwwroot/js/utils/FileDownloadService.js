// 负责浏览器文件下载动作，隔离临时链接创建细节。
export class FileDownloadService {
    // 通过临时 a 标签触发浏览器下载。
    static downloadFile(document, fileName, href) {
        const link = document.createElement("a");
        link.download = fileName;
        link.href = href;
        document.body.appendChild(link);
        link.click();
        link.remove();
    }
}
