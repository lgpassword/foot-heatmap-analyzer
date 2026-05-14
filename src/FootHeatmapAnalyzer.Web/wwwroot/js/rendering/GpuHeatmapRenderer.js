import { HeatmapRenderer } from "./HeatmapRenderer.js";

// 使用 WebGL shader 在浏览器本地渲染双三次插值热力图，WebGL 不可用时保留 Canvas 绘制路径。
export class GpuHeatmapRenderer {
    // 保存 DOM 文档对象，用于查找 Canvas。
    document;

    // 保存 Canvas 兼容渲染器。
    fallbackRenderer;

    // 创建 GPU 优先的热力图渲染器。
    constructor(document) {
        this.document = document;
        this.fallbackRenderer = new HeatmapRenderer(document);
    }

    // 绘制左右足合并热力图。
    drawCombinedHeatmap(canvasId, leftMatrix, rightMatrix, settings) {
        const canvas = this.document.getElementById(canvasId);
        if (!canvas || !leftMatrix?.length || !rightMatrix?.length) return;

        const gl = canvas.getContext("webgl2", { antialias: true, preserveDrawingBuffer: true });
        if (!gl) {
            this.fallbackRenderer.drawCombinedHeatmap(canvasId, leftMatrix, rightMatrix, settings);
            return;
        }

        const renderer = new WebGlBicubicRenderer(gl);
        if (!renderer.ready) {
            this.fallbackRenderer.drawCombinedHeatmap(canvasId, leftMatrix, rightMatrix, settings);
            return;
        }

        renderer.draw(leftMatrix, rightMatrix, settings);
    }
}

// 封装 WebGL 程序、纹理上传和 shader 绘制。
class WebGlBicubicRenderer {
    // 保存 WebGL 上下文。
    gl;

    // 保存编译后的 shader 程序。
    program;

    // 标记 WebGL 资源是否初始化成功。
    ready;

    // 创建 WebGL 双三次插值渲染器。
    constructor(gl) {
        this.gl = gl;
        this.program = this.createProgram();
        this.ready = Boolean(this.program);
    }

    // 绘制当前左右足压力矩阵。
    draw(leftMatrix, rightMatrix, settings) {
        const gl = this.gl;
        const program = this.program;
        gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);
        gl.clearColor(1, 1, 1, 1);
        gl.clear(gl.COLOR_BUFFER_BIT);
        gl.useProgram(program);

        const leftTexture = this.createTexture(leftMatrix);
        const rightTexture = this.createTexture(rightMatrix);
        this.bindQuad(program);
        this.drawFoot(program, leftTexture, 0, leftMatrix[0].length, leftMatrix.length, settings.contrast);
        this.drawFoot(program, rightTexture, 1, rightMatrix[0].length, rightMatrix.length, settings.contrast);
        gl.deleteTexture(leftTexture);
        gl.deleteTexture(rightTexture);
    }

    // 创建 8-bit 归一化纹理来承载压力矩阵，避免依赖浮点纹理扩展。
    createTexture(matrix) {
        const gl = this.gl;
        const width = matrix[0].length;
        const height = matrix.length;
        const data = new Uint8Array(width * height);
        for (let y = 0; y < height; y++) {
            for (let x = 0; x < width; x++) {
                data[(y * width) + x] = Math.max(0, Math.min(255, Math.round(matrix[y][x] * 255)));
            }
        }

        const texture = gl.createTexture();
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
        gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
        gl.texImage2D(gl.TEXTURE_2D, 0, gl.R8, width, height, 0, gl.RED, gl.UNSIGNED_BYTE, data);
        return texture;
    }

    // 绘制单只脚的 viewport 区域。
    drawFoot(program, texture, sideIndex, width, height, contrast) {
        const gl = this.gl;
        gl.activeTexture(gl.TEXTURE0);
        gl.bindTexture(gl.TEXTURE_2D, texture);
        gl.uniform1i(gl.getUniformLocation(program, "uTexture"), 0);
        gl.uniform2f(gl.getUniformLocation(program, "uMatrixSize"), width, height);
        gl.uniform1f(gl.getUniformLocation(program, "uSide"), sideIndex);
        gl.uniform1f(gl.getUniformLocation(program, "uContrast"), contrast);
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, 4);
    }

    // 绑定全屏四边形顶点。
    bindQuad(program) {
        const gl = this.gl;
        const vertices = new Float32Array([-1, -1, 1, -1, -1, 1, 1, 1]);
        const buffer = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, buffer);
        gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.STATIC_DRAW);

        const location = gl.getAttribLocation(program, "aPosition");
        gl.enableVertexAttribArray(location);
        gl.vertexAttribPointer(location, 2, gl.FLOAT, false, 0, 0);
    }

    // 创建 shader 程序。
    createProgram() {
        const gl = this.gl;
        const vertexShader = this.compile(gl.VERTEX_SHADER, vertexSource);
        const fragmentShader = this.compile(gl.FRAGMENT_SHADER, fragmentSource);
        if (!vertexShader || !fragmentShader) return null;

        const program = gl.createProgram();
        gl.attachShader(program, vertexShader);
        gl.attachShader(program, fragmentShader);
        gl.linkProgram(program);
        return gl.getProgramParameter(program, gl.LINK_STATUS) ? program : null;
    }

    // 编译单个 shader。
    compile(type, source) {
        const gl = this.gl;
        const shader = gl.createShader(type);
        gl.shaderSource(shader, source);
        gl.compileShader(shader);
        return gl.getShaderParameter(shader, gl.COMPILE_STATUS) ? shader : null;
    }
}

const vertexSource = `#version 300 es
in vec2 aPosition;
out vec2 vUv;
void main() {
    vUv = (aPosition + 1.0) * 0.5;
    gl_Position = vec4(aPosition, 0.0, 1.0);
}`;

const fragmentSource = `#version 300 es
precision highp float;
uniform sampler2D uTexture;
uniform vec2 uMatrixSize;
uniform float uSide;
uniform float uContrast;
in vec2 vUv;
out vec4 outColor;

float cubic(float v) {
    v = abs(v);
    float v2 = v * v;
    float v3 = v2 * v;
    if (v <= 1.0) return (1.5 * v3) - (2.5 * v2) + 1.0;
    if (v < 2.0) return (-0.5 * v3) + (2.5 * v2) - (4.0 * v) + 2.0;
    return 0.0;
}

float sampleBicubic(vec2 uv) {
    vec2 pixel = uv * uMatrixSize - 0.5;
    vec2 basePixel = floor(pixel);
    vec2 fraction = pixel - basePixel;
    float value = 0.0;
    float total = 0.0;
    for (int y = -1; y <= 2; y++) {
        for (int x = -1; x <= 2; x++) {
            vec2 offset = vec2(float(x), float(y));
            vec2 coord = clamp((basePixel + offset + 0.5) / uMatrixSize, vec2(0.0), vec2(1.0));
            float weight = cubic(offset.x - fraction.x) * cubic(fraction.y - offset.y);
            value += texture(uTexture, coord).r * weight;
            total += weight;
        }
    }
    return clamp(value / max(total, 0.0001), 0.0, 1.0);
}

float footHalfWidth(float ny) {
    if (ny < 0.20) return 0.52 + (ny * 1.1);
    if (ny < 0.38) return 0.76;
    if (ny < 0.62) return 0.48;
    if (ny < 0.82) return 0.55;
    return 0.42;
}

bool insideFootMask(float nx, float ny) {
    float width = footHalfWidth(ny);
    bool medialNotch = ny > 0.42 && ny < 0.74 && (uSide < 0.5 ? nx > 0.12 : nx < -0.12);
    if (abs(nx) > width || medialNotch) return false;
    if (ny < 0.13) {
        float toe = pow(nx / 0.58, 2.0) + pow((ny - 0.13) / 0.13, 2.0);
        return toe <= 1.0;
    }
    if (ny > 0.86) {
        float heel = pow(nx / 0.48, 2.0) + pow((ny - 0.86) / 0.15, 2.0);
        return heel <= 1.0;
    }
    return true;
}

vec3 colorFor(float value) {
    if (value >= 0.82) return vec3(0.91, 0.20, 0.09);
    if (value >= 0.64) return vec3(0.95, 0.50, 0.11);
    if (value >= 0.46) return vec3(0.95, 0.71, 0.10);
    if (value >= 0.28) return vec3(0.14, 0.65, 0.42);
    return vec3(0.11, 0.40, 0.90);
}

void main() {
    float panel = uSide < 0.5 ? 0.25 : 0.75;
    vec2 local = vec2((vUv.x - panel) * 2.6 + 0.5, vUv.y);
    float nx = (local.x * 2.0) - 1.0;
    float ny = local.y;
    if (local.x < 0.0 || local.x > 1.0 || !insideFootMask(nx, ny)) {
        discard;
    }

    float value = clamp((sampleBicubic(local) - 0.5) * uContrast + 0.5, 0.0, 1.0);
    outColor = vec4(colorFor(value), 1.0);
}`;
