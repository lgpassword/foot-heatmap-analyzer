// 负责读取和应用热力图显示参数。
export class SettingsController {
    // 保存当前页面文档对象。
    document;

    // 保存共享显示参数对象。
    settings;

    // 保存设置变更后的重绘回调。
    onChange;

    // 保存提示服务。
    toastService;

    // 创建设置控制器。
    constructor(document, settings, onChange, toastService) {
        this.document = document;
        this.settings = settings;
        this.onChange = onChange;
        this.toastService = toastService;
    }

    // 绑定所有设置输入控件。
    bind() {
        const settingInputs = this.document.querySelectorAll("[data-setting]");
        for (const input of settingInputs) {
            input.addEventListener("input", () => {
                this.readSettingsFromInputs();
                this.onChange();
            });
        }

        this.document.addEventListener("foot-heatmap-reset-settings", () => this.resetSettings());
    }

    // 从设置面板读取最新参数。
    readSettingsFromInputs() {
        const dotSize = this.document.querySelector("[data-setting='dotSize']");
        const dotSpacing = this.document.querySelector("[data-setting='dotSpacing']");
        const contrast = this.document.querySelector("[data-setting='contrast']");
        const showLabels = this.document.querySelector("[data-setting='showLabels']");

        this.settings.dotSize = this.numberFromInput(dotSize, 2);
        this.settings.dotSpacing = this.numberFromInput(dotSpacing, 6);
        this.settings.contrast = this.numberFromInput(contrast, 1);
        this.settings.showLabels = Boolean(showLabels?.checked);
    }

    // 恢复默认显示参数并更新控件。
    resetSettings() {
        const defaults = { dotSize: 2, dotSpacing: 6, contrast: 1, showLabels: true };
        Object.assign(this.settings, defaults);

        for (const input of this.document.querySelectorAll("[data-setting]")) {
            if (input.dataset.setting === "showLabels") input.checked = defaults.showLabels;
            if (input.dataset.setting === "dotSize") input.value = defaults.dotSize;
            if (input.dataset.setting === "dotSpacing") input.value = defaults.dotSpacing;
            if (input.dataset.setting === "contrast") input.value = defaults.contrast;
        }

        this.onChange();
        this.toastService.show("显示参数已恢复默认。");
    }

    // 把输入框值转换为数字，缺失时使用兜底值。
    numberFromInput(input, fallback) {
        const value = Number(input?.value);
        return Number.isFinite(value) ? value : fallback;
    }
}
