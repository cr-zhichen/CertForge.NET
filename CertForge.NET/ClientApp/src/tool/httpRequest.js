//axios
import axios from 'axios';

const config = {
    "root": "/api/certificate/root",
    "generate": "/api/certificate/generate",
}

//获取根证书
export function GetRootCertificate(ok, err) {
    // 获取当前网页的完整URL
    let currentURL = window.location.href;
    // 使用URL对象解析URL
    let urlObject = new URL(currentURL);
    // 获取URL的前缀（协议 + 主机）
    let urlPrefix = urlObject.protocol + '//' + urlObject.host;

    GetHelp(urlPrefix + config.root, ok, err);
}

//生成证书
export function GenerateCertificate(data, ok, err) {
    // 获取当前网页的完整URL
    let currentURL = window.location.href;
    // 使用URL对象解析URL
    let urlObject = new URL(currentURL);
    // 获取URL的前缀（协议 + 主机）
    let urlPrefix = urlObject.protocol + '//' + urlObject.host;

    PostHelp(urlPrefix + config.generate, data, ok, err);
}


//Get请求
export async function GetHelp(url, ok, err, token = '') {
    const loading = ElLoading.service({
        lock: true,
        text: 'Loading',
        background: 'rgba(0, 0, 0, 0.7)',
    })
    try {
        const response = await axios.get(url, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            }
        });

        if (response.status !== 200) {
            err && err(response.statusText);
            loading.close();
            return;
        }

        if (response.data.code !== 0) {
            err && err(response.data.message);
            loading.close();
            return;
        }

        ok(response.data);
    } catch (error) {
        err && err(error.message);
    }
    loading.close();
}

//Post请求
export async function PostHelp(url, data, ok, err, token = '') {
    const loading = ElLoading.service({
        lock: true,
        text: 'Loading',
        background: 'rgba(0, 0, 0, 0.7)',
    })

    try {
        const response = await axios.post(url, data, {
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            }
        });

        if (response.status !== 200) {
            err && err(response.statusText);
            loading.close();
            return;
        }

        if (response.data.code !== 0) {
            err && err(response.data.message);
            loading.close();
            return;
        }

        ok(response.data);
    } catch (error) {
        err && err(error.message);
    }

    loading.close();
}
