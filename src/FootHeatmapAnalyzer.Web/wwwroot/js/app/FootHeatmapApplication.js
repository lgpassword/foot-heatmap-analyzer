import { HeatmapDataStore } from "../data/HeatmapDataStore.js";
import { GpuHeatmapRenderer } from "../rendering/GpuHeatmapRenderer.js";
import { DistributionChartRenderer } from "../rendering/DistributionChartRenderer.js";
import { DashboardChartRenderer } from "../rendering/DashboardChartRenderer.js";
import { RegionLoadRenderer } from "../rendering/RegionLoadRenderer.js";
import { ActionController } from "../ui/ActionController.js";
import { HeatmapTabController } from "../ui/HeatmapTabController.js";
import { SettingsController } from "../ui/SettingsController.js";
import { ToastService } from "../ui/ToastService.js";
import { UploadController } from "../ui/UploadController.js";
import { ViewNavigationController } from "../ui/ViewNavigationController.js";

// 负责组装页面所需的前端服务，避免单个脚本承担所有职责。
export class FootHeatmapApplication {
    // 保存当前页面文档对象，便于测试和后续替换容器。
    document;

    // 保存扫描热力图数据读取服务。
    dataStore;

    // 保存合并足底热力图渲染器。
    heatmapRenderer;

    // 保存压力分布曲线渲染器。
    distributionRenderer;

    // 保存区域负载卡片渲染器。
    regionRenderer;

    // 保存 ECharts 仪表盘渲染器。
    dashboardRenderer;

    // 保存当前显示参数，所有绘图模块共用。
    settings;

    // 创建页面应用并初始化依赖对象。
    constructor(document) {
        this.document = document;
        this.settings = {
            dotSize: 2,
            dotSpacing: 6,
            contrast: 1,
            showLabels: true
        };
        this.dataStore = new HeatmapDataStore(document);
        this.heatmapRenderer = new GpuHeatmapRenderer(document);
        this.distributionRenderer = new DistributionChartRenderer(document);
        this.dashboardRenderer = new DashboardChartRenderer(document);
        this.regionRenderer = new RegionLoadRenderer(document);
    }

    // 启动页面全部功能控制器。
    start() {
        this.renderVisuals();

        const toastService = new ToastService(this.document);
        const settingsController = new SettingsController(this.document, this.settings, () => this.renderVisuals(), toastService);

        new HeatmapTabController(this.document, () => this.renderVisuals()).bind();
        new ViewNavigationController(this.document).bind();
        new UploadController(this.document).bind();
        settingsController.bind();
        new ActionController(this.document, this.dataStore, this.settings, () => this.renderVisuals(), toastService).bind();
    }

    // 重绘所有由扫描数据和显示参数驱动的视图。
    renderVisuals() {
        const payload = this.dataStore.getPayload();
        if (!payload) return;

        this.heatmapRenderer.drawCombinedHeatmap("combinedHeatmap", payload.left, payload.right, this.settings);
        this.distributionRenderer.drawPressureDistribution("pressureDistribution", payload.left, payload.right);
        this.dashboardRenderer.render("dashboardCharts", payload.left, payload.right);
        this.regionRenderer.renderRegionCards("regionLoadCards", payload.left, payload.right);
    }
}
