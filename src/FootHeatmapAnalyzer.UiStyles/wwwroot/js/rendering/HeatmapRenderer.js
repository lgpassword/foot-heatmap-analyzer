import { CanvasShapeUtils } from "./CanvasShapeUtils.js";
import { HeatmapMath } from "./HeatmapMath.js";

// 负责绘制左右足合并点阵热力图，不处理导航、导出或上传。
export class HeatmapRenderer {
    // 保存当前页面文档对象，用于查找 Canvas。
    document;

    // 创建热力图渲染器。
    constructor(document) {
        this.document = document;
    }

    // 绘制左右足同框热力图。
    drawCombinedHeatmap(canvasId, leftMatrix, rightMatrix, settings) {
        const canvas = this.document.getElementById(canvasId);
        if (!canvas || !leftMatrix?.length || !rightMatrix?.length) return;

        const ctx = canvas.getContext("2d");
        const panelWidth = 315;
        const panelHeight = 426;
        const panelGap = 34;
        const top = 40;
        const left = (canvas.width - ((panelWidth * 2) + panelGap)) / 2;

        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.fillStyle = "#ffffff";
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        this.drawFootPanel(ctx, left, top, panelWidth, panelHeight, leftMatrix, "左足", "LEFT", "left", settings);
        this.drawFootPanel(ctx, left + panelWidth + panelGap, top, panelWidth, panelHeight, rightMatrix, "右足", "RIGHT", "right", settings);
    }

    // 绘制单只脚的面板、点阵、色标和足别说明。
    drawFootPanel(ctx, x, y, width, height, matrix, title, label, side, settings) {
        const plotBox = {
            x: x + 42,
            y: y + 42,
            width: width - 88,
            height: height - 96
        };
        const scaleBox = {
            x: x + width - 48,
            y: y + 70,
            width: 12,
            height: height - 158
        };

        this.drawPanelChrome(ctx, x, y, width, height, title, settings.showLabels);
        this.drawDottedFoot(ctx, matrix, plotBox, side, settings);
        if (settings.showLabels) {
            this.drawColorScale(ctx, scaleBox);
            this.drawFootCaption(ctx, label, x + (width / 2), y + height - 28);
        }
    }

    // 绘制足底图内部浅色面板和标题。
    drawPanelChrome(ctx, x, y, width, height, title, showLabels) {
        ctx.fillStyle = "#f7fafd";
        CanvasShapeUtils.roundRect(ctx, x, y, width, height, 4);
        ctx.fill();
        ctx.strokeStyle = "#d8e2ef";
        ctx.lineWidth = 1;
        ctx.stroke();

        if (!showLabels) return;

        ctx.fillStyle = "#143560";
        ctx.font = "700 17px system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(title, x + (width / 2), y + 22);
    }

    // 按足形遮罩绘制压力点阵。
    drawDottedFoot(ctx, matrix, box, side, settings) {
        const range = HeatmapMath.matrixRange(matrix);
        const spacing = settings.dotSpacing;
        const radius = settings.dotSize;
        const footWidth = box.width * 0.68;
        const footHeight = box.height * 0.9;
        const centerX = box.x + (box.width / 2);
        const startY = box.y + (box.height - footHeight) / 2;

        ctx.fillStyle = "#eef4fb";
        CanvasShapeUtils.roundRect(ctx, box.x, box.y, box.width, box.height, 4);
        ctx.fill();
        ctx.strokeStyle = "#d8e2ef";
        ctx.stroke();

        for (let py = 0; py <= footHeight; py += spacing) {
            for (let px = -footWidth / 2; px <= footWidth / 2; px += spacing) {
                const nx = px / (footWidth / 2);
                const ny = py / footHeight;
                if (!HeatmapMath.insideFootMask(nx, ny, side)) continue;

                const value = HeatmapMath.sampleMatrix(matrix, (nx + 1) / 2, ny);
                const normalized = HeatmapMath.normalizeValue(value, range, settings.contrast);
                ctx.fillStyle = HeatmapMath.colorFor(normalized);
                ctx.beginPath();
                ctx.arc(centerX + px, startY + py, radius, 0, Math.PI * 2);
                ctx.fill();
            }
        }
    }

    // 绘制高低压力色标。
    drawColorScale(ctx, box) {
        const gradient = ctx.createLinearGradient(0, box.y, 0, box.y + box.height);
        gradient.addColorStop(0, "#e83218");
        gradient.addColorStop(0.35, "#f2a51a");
        gradient.addColorStop(0.62, "#28a66a");
        gradient.addColorStop(1, "#1d66e5");

        ctx.fillStyle = gradient;
        CanvasShapeUtils.roundRect(ctx, box.x, box.y, box.width, box.height, 2);
        ctx.fill();

        ctx.fillStyle = "#6a7a92";
        ctx.font = "10px 'IBM Plex Mono', Consolas, monospace";
        ctx.textAlign = "center";
        ctx.fillText("高", box.x + (box.width / 2), box.y - 9);
        ctx.fillText("低", box.x + (box.width / 2), box.y + box.height + 14);
    }

    // 绘制足别英文标签。
    drawFootCaption(ctx, label, centerX, y) {
        ctx.fillStyle = "#6a7a92";
        ctx.font = "600 11px 'IBM Plex Mono', Consolas, monospace";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(label, centerX, y);
    }
}
