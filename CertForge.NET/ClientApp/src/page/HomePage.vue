<script setup>
import {ref} from "@vue/reactivity";
import {getThemeIcon} from "@/tool/themeChange.js";
import {GenerateCertificate, GetRootCertificate} from "@/tool/httpRequest.js";
import {ElMessage} from "element-plus";

const theme = ref(getThemeIcon());
const formModel = ref({
    c: 'CN',
    o: 'CertForgeDotNET',
    cn: '',
    san: '',
    validityYear: 100
});
const publicKey = ref('');
const privateKey = ref('');

const formRef = ref(null);

const rules = {
    cn: [{required: true, message: '请输入通用名称', trigger: 'blur'}],
    validityYear: [
        {required: true, message: '有效时间不能为空', trigger: 'blur'},
        {type: 'number', message: '有效时间必须是数字', trigger: 'blur'}
    ]
};

const dialogTableVisible = ref(false);
const password = ref('');

//下载根证书
const downloadRoot = () => {
    GetRootCertificate((data) => {
        const publicKey = data.data.publicKey;
        // 创建 Blob 对象
        const blob = new Blob([publicKey], {type: 'application/x-x509-ca-cert'});
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = 'root.pem';

        // 将链接添加到文档
        document.body.appendChild(link);

        // 模拟用户点击下载链接
        link.click();
    }, (err) => {
        ElMessage.error(err);
    });
}

// 生成证书，包括C，O，CN，validityYear。
const generateCertificate = () => {
    // 弹窗要求输入密码
    dialogTableVisible.value = true;
}

// 发送证书请求
const sendCertificateRequest = () => {
    // 发送证书请求
    GenerateCertificate({
        c: formModel.value.c,
        o: formModel.value.o,
        cn: formModel.value.cn,
        san: formModel.value.san,
        validityYear: formModel.value.validityYear,
        password: password.value
    }, (data) => {
        publicKey.value = data.data.publicKey;
        privateKey.value = data.data.privateKey;
    }, (err) => {
        ElMessage.error(err);
    });
}

//下载公钥
const downloadPublicKey = () => {
    if (formModel.value.cn === '') {
        ElMessage.error('请先生成证书');
        return;
    }
    // 创建 Blob 对象
    const blob = new Blob([publicKey.value], {type: 'application/x-x509-ca-cert'});
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = formModel.value.cn + '.pem';

    // 将链接添加到文档
    document.body.appendChild(link);

    // 模拟用户点击下载链接
    link.click();
}

//下载私钥
const downloadPrivateKey = () => {
    if (formModel.value.cn === '') {
        ElMessage.error('请先生成证书');
        return;
    }
    // 创建 Blob 对象
    const blob = new Blob([privateKey.value], {type: 'application/x-x509-ca-cert'});
    const link = document.createElement('a');
    link.href = window.URL.createObjectURL(blob);
    link.download = formModel.value.cn + '.key';

    // 将链接添加到文档
    document.body.appendChild(link);

    // 模拟用户点击下载链接
    link.click();
}

</script>

<template>
    <div id="homePage">
        <h1>{{ $t('homePage.title') }}</h1>

        <el-form ref="formRef" :model="formModel" :rules="rules" label-width="120px" label-position="top">
            <el-form-item label="C (国家)">
                <el-input v-model="formModel.c" placeholder="CN"></el-input>
            </el-form-item>
            <el-form-item label="O (组织)">
                <el-input v-model="formModel.o" placeholder="CertForge.NET"></el-input>
            </el-form-item>
            <el-form-item label="CN (通用名称)" prop="cn">
                <el-input v-model="formModel.cn" placeholder="127.0.0.1"></el-input>
            </el-form-item>
            <el-form-item label="SAN (主机名,多主机名使用逗号分隔,为空则使用CN作为SAN)">
                <el-input v-model="formModel.san" placeholder="192.168.1.1,192.168.1.2"></el-input>
            </el-form-item>
            <el-form-item label="有效时间 (年)" prop="validityYear">
                <el-input v-model.number="formModel.validityYear" placeholder="100"></el-input>
            </el-form-item>
            <el-form-item>
                <el-button
                    type="primary"
                    @click="generateCertificate"
                    class="homePage-generateCertificateBtn"
                    icon="Promotion"
                >
                    生成证书
                </el-button>
            </el-form-item>
        </el-form>

        <div class="homePage-certificate" style="display: flex; justify-content: space-between; width: 100%;">
            <el-card
                style="flex-grow: 1; margin-right: 10px; height: 300px; display: flex; flex-direction: column; justify-content: space-between; overflow: auto;">
                <template #header>
                    <div>证书</div>
                </template>
                <pre>{{ publicKey }}</pre>
                <el-button
                    @click="downloadPublicKey"
                    icon="Download"
                    class="homePage-downloadRootBtn"
                    type="primary"
                >
                    下载公钥
                </el-button>
            </el-card>

            <el-card
                style="flex-grow: 1; margin-right: 10px; height: 300px; display: flex; flex-direction: column; justify-content: space-between; overflow: auto;">
                <template #header>
                    <div>私钥</div>
                </template>
                <pre>{{ privateKey }}</pre>
                <el-button
                    @click="downloadPrivateKey"
                    icon="Download"
                    class="homePage-downloadRootBtn"
                    type="primary"
                >
                    下载私钥
                </el-button>
            </el-card>
        </div>

        <el-button
            @click="downloadRoot"
            icon="Download"
            class="homePage-downloadRootBtn"
            type="primary"
        >
            下载根证书
        </el-button>

        <el-dialog
            v-model="dialogTableVisible"
            title="请输入密码"
            width="800px">

            <!-- 密码输入框 -->
            <el-input
                v-model="password"
                placeholder="请输入密码"
                type="password"
                show-password>
            </el-input>

            <!-- 底部操作按钮 -->
            <template #footer>
            <span class="dialog-footer">
                <el-button @click="dialogTableVisible = false">取消</el-button>
                <el-button type="primary" @click="sendCertificateRequest">确定</el-button>
            </span>
            </template>
        </el-dialog>


    </div>
</template>

<style scoped>

.homePage-downloadRootBtn {
    margin-bottom: 20px;
    width: 100%;
}

.homePage-generateCertificateBtn {
    width: 100%;
}

.homePage-certificate {
    margin-bottom: 20px;
}

#homePage {
    width: 100%;
}

</style>