// 使用后端 ECharts 配置渲染压力分析仪表盘。
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
            <div class="dashboard-chart" data-chart="balance"></div>
            <div class="dashboard-chart" data-chart="cop"></div>
            <div class="dashboard-chart" data-chart="loadDist"></div>
            <div class="dashboard-chart" data-chart="hotspot"></div>`;
        void this.renderFromApi(container, leftMatrix, rightMatrix);
    }

    // 从后端 API 获取真实分析值并写入四个图表。
    async renderFromApi(container, leftMatrix, rightMatrix) {
        const response = await fetch("/api/dashboard", {
            method: "POST",
            headers: { "Content-Type": "text/plain" },
            body: this.encodeCurrentPayloadAsHex(leftMatrix, rightMatrix)
        });
        if (!response.ok) {
            container.textContent = "仪表盘数据生成失败";
            return;
        }

        const payload = await response.json();
        this.renderChart(container.querySelector('[data-chart="balance"]'), "左右受力平衡", payload.balanceChart);
        this.renderChart(container.querySelector('[data-chart="cop"]'), "压力中心偏移", payload.copChart);
        this.renderChart(container.querySelector('[data-chart="loadDist"]'), "前足/足跟分布", payload.loadDistChart);
        this.renderChart(container.querySelector('[data-chart="hotspot"]'), "热点数量", payload.hotspotChart);
    }

    // 给后端返回的 ECharts 配置补充中文标题后渲染。
    renderChart(element, title, option) {
        if (!element || !option) return;

        const chart = globalThis.echarts.init(element);
        chart.setOption({
            title: { text: title, left: 12, textStyle: { fontSize: 13 } },
            tooltip: {},
            ...option
        });
    }

    // 将当前矩阵编码为演示协议十六进制载荷。
    encodeCurrentPayloadAsHex(leftMatrix, rightMatrix) {
        const height = leftMatrix.length;
        const width = leftMatrix[0].length;
        const bytes = [width, height];
        for (const matrix of [leftMatrix, rightMatrix]) {
            for (const row of matrix) {
                for (const value of row) {
                    bytes.push(Math.max(0, Math.min(255, Math.round(value * 255))));
                }
            }
        }

        return bytes.map(value => value.toString(16).padStart(2, "0")).join("");
    }
}
