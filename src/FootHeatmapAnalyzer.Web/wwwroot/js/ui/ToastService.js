// 负责显示短提示，不参与业务逻辑。
export class ToastService {
    // 保存当前页面文档对象。
    document;

    // 保存当前隐藏定时器编号。
    timeoutId;

    // 创建提示服务。
    constructor(document) {
        this.document = document;
        this.timeoutId = 0;
    }

    // 显示一条短提示。
    show(message) {
        const toast = this.document.querySelector("[data-app-toast]");
        if (!toast) return;

        toast.textContent = message;
        toast.classList.add("show");
        window.clearTimeout(this.timeoutId);
        this.timeoutId = window.setTimeout(() => toast.classList.remove("show"), 2200);
    }
}
