// 负责左侧功能导航和页面级面板切换。
export class ViewNavigationController {
    // 保存当前页面文档对象。
    document;

    // 保存各功能页面的标题和面包屑文案。
    viewMeta = {
        analysis: { title: "足底压力热力图分析", crumb: "分析面板 / 热力图识别 / 当前样本" },
        history: { title: "历史对比", crumb: "分析面板 / 历史对比 / 当前会话" },
        export: { title: "导出报告", crumb: "分析面板 / 报告输出 / 当前样本" },
        settings: { title: "参数设置", crumb: "分析面板 / 显示参数 / 当前样本" }
    };

    // 创建导航控制器。
    constructor(document) {
        this.document = document;
    }

    // 绑定左侧导航按钮。
    bind() {
        const viewButtons = this.document.querySelectorAll("[data-view-target]");
        for (const button of viewButtons) {
            button.addEventListener("click", () => this.switchView(button.dataset.viewTarget ?? "analysis"));
        }
    }

    // 切换功能面板并同步顶部标题。
    switchView(viewName) {
        const selectedView = this.viewMeta[viewName] ? viewName : "analysis";

        for (const button of this.document.querySelectorAll("[data-view-target]")) {
            button.classList.toggle("active", button.dataset.viewTarget === selectedView);
        }

        for (const panel of this.document.querySelectorAll("[data-view-panel]")) {
            panel.classList.toggle("hidden", panel.dataset.viewPanel !== selectedView);
        }

        this.updateHeader(selectedView);
    }

    // 更新主标题和面包屑。
    updateHeader(selectedView) {
        const heading = this.document.querySelector("[data-page-heading]");
        const crumb = this.document.querySelector("[data-page-crumb]");
        if (heading) heading.innerHTML = `<i class="ti ti-activity" aria-hidden="true"></i> ${this.viewMeta[selectedView].title}`;
        if (crumb) crumb.innerHTML = this.viewMeta[selectedView].crumb.replace("当前样本", "<span>当前样本</span>");
    }
}
