// 封装矩阵采样、归一化和足形遮罩，保持渲染器代码聚焦绘图。
export class HeatmapMath {
    // 判断归一化坐标是否落在足形轮廓内。
    static insideFootMask(nx, ny, side) {
        const width = HeatmapMath.footHalfWidth(ny);
        const medialNotch = ny > 0.42 && ny < 0.74 && (side === "left" ? nx > 0.12 : nx < -0.12);
        const inOuterShape = Math.abs(nx) <= width;
        if (!inOuterShape || medialNotch) return false;

        if (ny < 0.13) {
            const toeDome = Math.pow(nx / 0.58, 2) + Math.pow((ny - 0.13) / 0.13, 2);
            return toeDome <= 1;
        }

        if (ny > 0.86) {
            const heelDome = Math.pow(nx / 0.48, 2) + Math.pow((ny - 0.86) / 0.15, 2);
            return heelDome <= 1;
        }

        return true;
    }

    // 根据垂直位置返回足形半宽，用于近似前足、中足和足跟轮廓。
    static footHalfWidth(ny) {
        if (ny < 0.20) return 0.52 + (ny * 1.1);
        if (ny < 0.38) return 0.76;
        if (ny < 0.62) return 0.48;
        if (ny < 0.82) return 0.55;
        return 0.42;
    }

    // 按归一化坐标从矩阵中采样一个压力值。
    static sampleMatrix(matrix, x, y) {
        const row = Math.max(0, Math.min(matrix.length - 1, Math.round(y * (matrix.length - 1))));
        const column = Math.max(0, Math.min(matrix[row].length - 1, Math.round(x * (matrix[row].length - 1))));
        return matrix[row][column];
    }

    // 计算矩阵最小值和最大值，用于当前样本内归一化。
    static matrixRange(matrix) {
        const range = { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY };
        for (const row of matrix) {
            for (const value of row) {
                range.min = Math.min(range.min, value);
                range.max = Math.max(range.max, value);
            }
        }

        return range;
    }

    // 将压力值按当前矩阵范围归一化，并应用显示对比度。
    static normalizeValue(value, range, contrast) {
        const span = Math.max(0.0001, range.max - range.min);
        const centered = ((value - range.min) / span) - 0.5;
        return Math.max(0, Math.min(1, (centered * contrast) + 0.5));
    }

    // 将归一化压力值映射到红黄绿蓝色阶。
    static colorFor(value) {
        const v = Math.max(0, Math.min(1, value));
        if (v >= 0.82) return "#e83218";
        if (v >= 0.64) return "#f27f1b";
        if (v >= 0.46) return "#f2b51a";
        if (v >= 0.28) return "#24a66a";
        return "#1d66e5";
    }

    // 计算每一行的平均压力，用于压力分布曲线。
    static rowAverages(matrix) {
        return matrix.map(row => row.reduce((sum, value) => sum + value, 0) / Math.max(1, row.length));
    }

    // 计算指定行区间的平均压力。
    static averageRows(matrix, startRow, endRow) {
        let total = 0;
        let count = 0;
        for (let rowIndex = startRow; rowIndex < endRow; rowIndex++) {
            for (const value of matrix[rowIndex] ?? []) {
                total += value;
                count++;
            }
        }

        return count > 0 ? total / count : 0;
    }

    // 把归一化数值格式化为百分比。
    static percent(value) {
        return `${Math.round(value * 100)}%`;
    }
}
