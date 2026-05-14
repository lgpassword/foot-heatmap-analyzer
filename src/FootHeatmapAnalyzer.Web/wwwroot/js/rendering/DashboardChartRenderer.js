import { HeatmapMath } from "./HeatmapMath.js";

// 使用 ECharts 渲染压力中心偏移、步频代理和左右受力平衡报表。
export class DashboardChartRenderer {
    // 保存当前页面文档对象。
    document;

    // 创建仪表盘渲染器。
    constructor(document) {
        this.document = document;
    }

    // 渲染全部仪表盘图表。
    render(containerId, leftMatrix, rightMatrix) {
        const container = this.document.getElementById(containerId);
        if (!container || !globalThis.echarts || !leftMatrix?.length || !rightMatrix?.length) return;

        container.innerHTML = `
            <div class="dashboard-chart" data-chart="cop"></div>
            <div class="dashboard-chart" data-chart="cadence"></div>
            <div class="dashboard-chart" data-chart="balance"></div>`;
        this.renderCop(container.querySelector('[data-chart="cop"]'), leftMatrix, rightMatrix);
        this.renderCadence(container.querySelector('[data-chart="cadence"]'), leftMatrix, rightMatrix);
        this.renderBalance(container.querySelector('[data-chart="balance"]'), leftMatrix, rightMatrix);
    }

    // 渲染压力中心偏移柱状图。
    renderCop(element, leftMatrix, rightMatrix) {
        const chart = globalThis.echarts.init(element);
        const leftCenter = this.centerOfPressure(leftMatrix);
        const rightCenter = this.centerOfPressure(rightMatrix);
        chart.setOption({
            title: { text: "CoP 偏移", left: 12, textStyle: { fontSize: 13 } },
            xAxis: { type: "category", data: ["左X", "左Y", "右X", "右Y"] },
            yAxis: { type: "value" },
            series: [{ type: "bar", data: [leftCenter.x, leftCenter.y, rightCenter.x, rightCenter.y] }]
        });
    }

    // 渲染步频代理趋势图。
    renderCadence(element, leftMatrix, rightMatrix) {
        const chart = globalThis.echarts.init(element);
        const forefoot = (HeatmapMath.averageRows(leftMatrix, 0, 4) + HeatmapMath.averageRows(rightMatrix, 0, 4)) / 2;
        const heel = (HeatmapMath.averageRows(leftMatrix, 8, leftMatrix.length) + HeatmapMath.averageRows(rightMatrix, 8, rightMatrix.length)) / 2;
        chart.setOption({
            title: { text: "步频分析", left: 12, textStyle: { fontSize: 13 } },
            xAxis: { type: "category", data: ["足跟", "过渡", "前足"] },
            yAxis: { type: "value" },
            series: [{ type: "line", smooth: true, data: [heel * 120, ((heel + forefoot) / 2) * 120, forefoot * 120] }]
        });
    }

    // 渲染左右脚受力平衡仪表图。
    renderBalance(element, leftMatrix, rightMatrix) {
        const chart = globalThis.echarts.init(element);
        const leftLoad = leftMatrix.flat().reduce((sum, value) => sum + value, 0);
        const rightLoad = rightMatrix.flat().reduce((sum, value) => sum + value, 0);
        const leftPercent = Math.round((leftLoad / Math.max(leftLoad + rightLoad, 0.001)) * 100);
        chart.setOption({
            title: { text: "左右受力", left: 12, textStyle: { fontSize: 13 } },
            series: [{ type: "gauge", progress: { show: true }, data: [{ value: leftPercent, name: "左脚%" }] }]
        });
    }

    // 计算矩阵压力中心相对居中位置。
    centerOfPressure(matrix) {
        let load = 0;
        let xSum = 0;
        let ySum = 0;
        for (let y = 0; y < matrix.length; y++) {
            for (let x = 0; x < matrix[y].length; x++) {
                const value = matrix[y][x];
                load += value;
                xSum += x * value;
                ySum += y * value;
            }
        }

        return {
            x: Math.round(((xSum / Math.max(load, 0.001) / Math.max(matrix[0].length - 1, 1)) - 0.5) * 100),
            y: Math.round(((ySum / Math.max(load, 0.001) / Math.max(matrix.length - 1, 1)) - 0.5) * 100)
        };
    }
}
