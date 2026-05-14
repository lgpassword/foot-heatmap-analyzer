// 负责热力图卡片内部页签切换。
export class HeatmapTabController {
    // 保存当前页面文档对象。
    document;

    // 保存切换页签后的重绘回调。
    onChange;

    // 创建页签控制器。
    constructor(document, onChange) {
        this.document = document;
        this.onChange = onChange;
    }

    // 绑定所有热力图页签按钮。
    bind() {
        const tabButtons = this.document.querySelectorAll("[data-viz-tab]");
        for (const button of tabButtons) {
            button.addEventListener("click", () => this.switchHeatmapTab(button.dataset.vizTab ?? "combined"));
        }
    }

    // 切换当前热力图页签和对应面板。
    switchHeatmapTab(tabName) {
        for (const button of this.document.querySelectorAll("[data-viz-tab]")) {
            const selected = button.dataset.vizTab === tabName;
            button.classList.toggle("tab-active", selected);
            button.setAttribute("aria-selected", selected.toString());
        }

        for (const panel of this.document.querySelectorAll("[data-viz-panel]")) {
            panel.classList.toggle("hidden", panel.dataset.vizPanel !== tabName);
        }

        this.onChange();
    }
}
