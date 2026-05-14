import { HeatmapMath } from "./HeatmapMath.js";

// 负责绘制左右足压力分布曲线图。
export class DistributionChartRenderer {
    // 保存当前页面文档对象，用于查找图表 Canvas。
    document;

    // 创建压力分布图渲染器。
    constructor(document) {
        this.document = document;
    }

    // 绘制左右足行均值分布曲线。
    drawPressureDistribution(canvasId, leftMatrix, rightMatrix) {
        const canvas = this.document.getElementById(canvasId);
        if (!canvas || !leftMatrix?.length || !rightMatrix?.length) return;

        const ctx = canvas.getContext("2d");
        const padding = { left: 58, right: 24, top: 30, bottom: 42 };
        const chartWidth = canvas.width - padding.left - padding.right;
        const chartHeight = canvas.height - padding.top - padding.bottom;
        const leftProfile = HeatmapMath.rowAverages(leftMatrix);
        const rightProfile = HeatmapMath.rowAverages(rightMatrix);

        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = "#ffffff";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        this.drawChartFrame(ctx, padding, chartWidth, chartHeight);
        this.drawDistributionLine(ctx, leftProfile, padding, chartWidth, chartHeight, "#0c7b80", "左足");
        this.drawDistributionLine(ctx, rightProfile, padding, chartWidth, chartHeight, "#1a7fe0", "右足");
        this.drawChartLegend(ctx, canvas.width - 160, 18);
    }

    // 绘制坐标轴和水平辅助线。
    drawChartFrame(ctx, padding, chartWidth, chartHeight) {
        ctx.strokeStyle = "#d8e2ef";
        ctx.lineWidth = 1;
        for (let index = 0; index <= 4; index++) {
            const y = padding.top + (chartHeight * index / 4);
            ctx.beginPath();
            ctx.moveTo(padding.left, y);
            ctx.lineTo(padding.left + chartWidth, y);
            ctx.stroke();
        }

        ctx.fillStyle = "#4a5a72";
        ctx.font = "11px 'IBM Plex Mono', Consolas, monospace";
        ctx.textAlign = "center";
        ctx.fillText("前足", padding.left + 18, padding.top + chartHeight + 26);
        ctx.fillText("中足", padding.left + (chartWidth / 2), padding.top + chartHeight + 26);
        ctx.fillText("足跟", padding.left + chartWidth - 18, padding.top + chartHeight + 26);
    }

    // 绘制一条脚部压力曲线和采样点。
    drawDistributionLine(ctx, values, padding, chartWidth, chartHeight, color, label) {
        ctx.strokeStyle = color;
        ctx.fillStyle = color;
        ctx.lineWidth = 2;
        ctx.beginPath();

        values.forEach((value, index) => {
            const x = padding.left + (chartWidth * index / Math.max(1, values.length - 1));
            const y = padding.top + chartHeight - (value * chartHeight);
            if (index === 0) ctx.moveTo(x, y);
            else ctx.lineTo(x, y);
        });

        ctx.stroke();

        values.forEach((value, index) => {
            const x = padding.left + (chartWidth * index / Math.max(1, values.length - 1));
            const y = padding.top + chartHeight - (value * chartHeight);
            ctx.beginPath();
            ctx.arc(x, y, 3, 0, Math.PI * 2);
            ctx.fill();
        });

        ctx.font = "600 11px system-ui, sans-serif";
        ctx.fillText(label, padding.left + chartWidth - 24, padding.top + (label === "左足" ? 18 : 36));
    }

    // 绘制图例。
    drawChartLegend(ctx, x, y) {
        ctx.font = "11px system-ui, sans-serif";
        ctx.textAlign = "left";
        this.drawLegendLabel(ctx, x, y, "#0c7b80", "左足行均值");
        this.drawLegendLabel(ctx, x, y + 18, "#1a7fe0", "右足行均值");
    }

    // 绘制单个图例项。
    drawLegendLabel(ctx, x, y, color, label) {
        ctx.fillStyle = color;
        ctx.fillRect(x, y - 8, 10, 10);
        ctx.fillStyle = "#4a5a72";
        ctx.fillText(label, x + 16, y);
    }
}
