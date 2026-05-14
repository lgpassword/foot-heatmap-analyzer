import { HeatmapMath } from "./HeatmapMath.js";

// 负责生成前足、中足、足跟区域负载统计卡片。
export class RegionLoadRenderer {
    // 保存当前页面文档对象，用于查找卡片容器。
    document;

    // 创建区域负载渲染器。
    constructor(document) {
        this.document = document;
    }

    // 渲染左右足三段区域负载卡片。
    renderRegionCards(containerId, leftMatrix, rightMatrix) {
        const container = this.document.getElementById(containerId);
        if (!container) return;

        const leftRegions = this.regionMetrics(leftMatrix);
        const rightRegions = this.regionMetrics(rightMatrix);
        const labels = [
            { key: "forefoot", name: "前足" },
            { key: "midfoot", name: "中足" },
            { key: "heel", name: "足跟" }
        ];

        container.innerHTML = labels.map(region => this.regionCardHtml(region.name, leftRegions[region.key], rightRegions[region.key])).join("");
    }

    // 生成单个区域统计卡片的 HTML。
    regionCardHtml(name, leftValue, rightValue) {
        const average = Math.round(((leftValue + rightValue) / 2) * 100);
        return `
            <section class="region-card">
                <strong>${name}负载</strong>
                <div class="region-metric-row"><span>左足</span><b>${HeatmapMath.percent(leftValue)}</b></div>
                <div class="region-metric-row"><span>右足</span><b>${HeatmapMath.percent(rightValue)}</b></div>
                <div class="region-bar" aria-hidden="true"><span style="width:${average}%"></span></div>
            </section>`;
    }

    // 按前足、中足、足跟三个区域聚合矩阵均值。
    regionMetrics(matrix) {
        const height = matrix.length;
        const forefootEnd = Math.max(1, Math.floor(height * 0.35));
        const midfootEnd = Math.max(forefootEnd + 1, Math.floor(height * 0.7));

        return {
            forefoot: HeatmapMath.averageRows(matrix, 0, forefootEnd),
            midfoot: HeatmapMath.averageRows(matrix, forefootEnd, midfootEnd),
            heel: HeatmapMath.averageRows(matrix, midfootEnd, height)
        };
    }
}
