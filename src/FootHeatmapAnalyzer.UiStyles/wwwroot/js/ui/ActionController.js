import { HeatmapRenderer } from "../rendering/HeatmapRenderer.js";
import { FileDownloadService } from "../utils/FileDownloadService.js";

// 负责顶部按钮和导出页动作按钮，不包含具体绘图算法。
export class ActionController {
    // 保存当前页面文档对象。
    document;

    // 保存扫描数据仓储。
    dataStore;

    // 保存共享显示参数。
    settings;

    // 保存重绘回调。
    renderVisuals;

    // 保存提示服务。
    toastService;

    // 保存独立热力图渲染器，用于导出前刷新画布。
    heatmapRenderer;

    // 创建动作控制器。
    constructor(document, dataStore, settings, renderVisuals, toastService) {
        this.document = document;
        this.dataStore = dataStore;
        this.settings = settings;
        this.renderVisuals = renderVisuals;
        this.toastService = toastService;
        this.heatmapRenderer = new HeatmapRenderer(document);
    }

    // 绑定所有声明式动作按钮。
    bind() {
        const actionButtons = this.document.querySelectorAll("[data-app-action]");
        for (const button of actionButtons) {
            button.addEventListener("click", () => this.runAppAction(button.dataset.appAction ?? ""));
        }
    }

    // 根据动作名称分发到具体行为。
    runAppAction(actionName) {
        if (actionName === "reanalyze") {
            this.renderVisuals();
            this.toastService.show("已根据当前样本重新生成页面分析视图。");
            return;
        }

        if (actionName === "export-heatmap") {
            this.exportHeatmapPng();
            return;
        }

        if (actionName === "export-report") {
            this.exportReportText();
            return;
        }

        if (actionName === "print-report") {
            window.print();
            return;
        }

        if (actionName === "apply-settings") {
            this.renderVisuals();
            this.toastService.show("显示参数已应用到热力图。");
            return;
        }

        if (actionName === "reset-settings") {
            this.resetSettingsFromPanel();
            return;
        }

        if (actionName === "notify") {
            this.toastService.show("当前没有新的系统通知。");
            return;
        }

        if (actionName === "help") {
            this.toastService.show("上传 HEX、BIN 或 Base64 扫描数据后，页面会在本地完成分析。");
        }
    }

    // 导出当前左右足合并热力图 PNG。
    exportHeatmapPng() {
        const payload = this.dataStore.getPayload();
        const canvas = this.document.getElementById("combinedHeatmap");
        if (!payload || !canvas) return;

        this.heatmapRenderer.drawCombinedHeatmap("combinedHeatmap", payload.left, payload.right, this.settings);
        FileDownloadService.downloadFile(this.document, "foot-heatmap-combined.png", canvas.toDataURL("image/png"));
        this.toastService.show("已导出左右足合并热力图 PNG。");
    }

    // 导出当前报告预览文本。
    exportReportText() {
        const reportPreview = this.document.getElementById("reportPreview");
        const text = reportPreview?.innerText?.trim() || this.document.body.innerText.trim();
        const blob = new Blob([text], { type: "text/plain;charset=utf-8" });
        const href = URL.createObjectURL(blob);
        FileDownloadService.downloadFile(this.document, "foot-heatmap-report.txt", href);
        URL.revokeObjectURL(href);
        this.toastService.show("已导出当前文本报告。");
    }

    // 触发设置重置按钮对应的输入控件状态。
    resetSettingsFromPanel() {
        const resetEvent = new CustomEvent("foot-heatmap-reset-settings");
        this.document.dispatchEvent(resetEvent);
    }
}
