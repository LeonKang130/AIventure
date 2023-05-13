const express = require('express')
const { spawn } = require('child_process');
const app = express()

app.use(express.json())//配置解析表单请求体 只能解析json
app.use(express.urlencoded())//配置解析表单请求体 只能解析x-www-form

//运行python脚本
app.post('/chat', async(req, res) =>{ //req请求对象，res响应对象
    try{
        //1.获取数据
        // console.log("收到的req",req);
        // console.log("收到的body",req.body);
        const messages = req.body.messages   //获取到一个list
        console.log('chat Received messages:', messages);
        // res.status(200)
        // 2. 执行 Python 脚本并传递参数  
        const pythonProcess = spawn('python3', ['chat.py', JSON.stringify(messages)]);

        // 3. 处理 Python 脚本的输出
        pythonProcess.stdout.on('data', (data) => {
            const output = data.toString();
            console.log('Python script output:', output);
            res.status(200).send( output ); // 返回 Python 脚本的输出给前端
        });
    
        pythonProcess.stderr.on('data', (data) => {
            console.error(`Python script error: ${data}`);
            res.status(500).json({ error: 'Python script error' });
        });        
    }catch(err){
        console.error(`Server error: ${err.message}`);
        res.status(500).json({
            error: err.message
        })       
    }
})

//运行python脚本
app.post('/image', async(req, res) =>{ //req请求对象，res响应对象
    try{
        //1.获取数据
        console.log(req.body)
        const items_num = req.body.items_num  //获取到一个数量
        console.log('image Received messages:', items_num);
        // res.status(200)
        // 2. 执行 Python 脚本并传递参数  
        const pythonProcess = spawn('python3', ['image.py', JSON.stringify(items_num)]);

        // 3. 处理 Python 脚本的输出
        pythonProcess.stdout.on('data', (data) => {
            const output = data.toString();
            console.log('Python script output image url:', output);
            res.status(200).send(output); // 返回 Python 脚本的输出给前端
        });
    
        pythonProcess.stderr.on('data', (data) => {
            console.error(`Python script error: ${data}`);
            res.status(500).json({ error: 'Python script error' });
        });        
    }catch(err){
        console.error(`Server error: ${err.message}`);
        res.status(500).json({
            error: err.message
        })       
    }
})

app.listen(3000, ()=>{
    console.log('server running at http://localhost:3000/')
})

