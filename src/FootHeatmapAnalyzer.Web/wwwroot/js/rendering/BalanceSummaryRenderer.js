// 渲染右侧栏的左右负载平衡摘要，避免服务端长文本被窄栏截断。
export class BalanceSummaryRenderer {
    // 保存当前页面文档对象。
    document;

    // 创建左右负载摘要渲染器。
    constructor(document) {
        this.document = document;
    }

    // 根据当前左右足矩阵刷新摘要卡片。
    render(leftMatrix, rightMatrix) {
        const leftLoad = this.totalLoad(leftMatrix);
        const rightLoad = this.totalLoad(rightMatrix);
        const total = Math.max(leftLoad + rightLoad, 0.001);
        const leftPercent = Math.round((leftLoad / total) * 100);
        const rightPercent = 100 - leftPercent;
        const gap = Math.abs(leftPercent - rightPercent);
        const state = gap <= 8 ? "左右负载接近均衡" : leftPercent > rightPercent ? "左足负载偏高" : "右足负载偏高";

        this.setText("[data-balance-left]", `左足 ${leftPercent}%`);
        this.setText("[data-balance-right]", `右足 ${rightPercent}%`);
        this.setText("[data-balance-state]", state);
        this.setText("[data-balance-note]", `差异 ${gap}% · ${gap <= 8 ? "当前样本较对称" : "建议复核站姿与采集质量"}`);

        const fill = this.document.querySelector("[data-balance-fill-left]");
        if (fill) {
            fill.style.width = `${leftPercent}%`;
        }
    }

    // 计算矩阵总负载。
    totalLoad(matrix) {
        return matrix.flat().reduce((sum, value) => sum + value, 0);
    }

    // 设置指定元素文本。
    setText(selector, value) {
        const element = this.document.querySelector(selector);
        if (element) {
            element.textContent = value;
        }
    }
}
