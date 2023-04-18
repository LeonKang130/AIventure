const express = require('express')
const fs = require('fs')//文件加载模块
const { getDb ,saveDb } = require('./db')//加入文件封装模块
const app = express()

app.use(express.json())//配置解析表单请求体 只能解析json
app.use(express.urlencoded())//配置解析表单请求体 只能解析x-www-form

//获取所有角色对话列表
app.get('/chat', async(req, res) =>{ //req请求对象，res响应对象
    // res.send('hello! get')
    try{
        const db = await getDb()
        res.status(200).json(db.roles)
    }catch(err){
        res.status(500).json({
            error: err.message
        })
    }
})

//获取单个id的role信息
app.get('/chat/:id', async(req, res) =>{ // :后面是任意的
    try{
        const db = await getDb()

        const role = db.roles.find(role => role.id === Number.parseInt(req.params.id))

        if(!role) {
            return res.status(404).end()
        }
        //有数据
        res.status(200).json(role)
    }catch(err){
        res.status(500).json({
            error: err.message
        })        
    }
})

//添加任务(添加更多角色npc)
app.post('/chat', async(req, res) =>{ //req请求对象，res响应对象
    // res.send('hello post!')
    try{
        //1.获取客户端请求体参数
        console.log(req.body)//配置解析表单请求体app.use(express.json())
        const role = req.body
        //2.数据验证(必须有name和setting和dialogue字段)
        if(!role.name){//客户端错误
            return res.status(422).json({
                error: 'the field name is required.'
            })
        }
        if(!role.setting){
            return res.status(422).json({
                error: 'the field setting is required.'
            })            
        }
        if(!role.dialogue){
            return res.status(422).json({
                error: 'the field dialogue is required.'
            })            
        }
        //3.数据验证通过，把数据储存到db中
        const db = await getDb()
        const lastrole = db.roles[db.roles.length - 1] //防止为空数组
        role.id = lastrole ? Number.parseInt(lastrole.id) + 1 : 1//Number.parseInt确保是字符串转数字
        db.roles.push(role)
        await saveDb(db)
        //4.发送响应
        res.status(201).json(role)//添加成功201
    }catch(err){
        res.status(500).json({
            error: err.message
        })
    }
})

//修改更新与npc的对话内容
app.patch('/chat/:id', async(req, res) =>{ //req请求对象，res响应对象
    // res.send('put user')
    try{
        //1.获取表单数据
        const role = req.body   
        //2.查找到要修改的任务项
        const db = await getDb()
        const result = db.roles.find(role => role.id === Number.parseInt(req.params.id)) 
        if(!result){
            return res.status(404).end()
        }
        Object.assign(result, role)//role提交数据 result查到的数据 将role合并到result

        await saveDb(db)

        res.status(200).json(result)//将合并修改后的数据发送
                
    }catch(err){
        res.status(500).json({
            error: err.message
        })        
    }
})

//删除npc
app.delete('/chat/:id', async(req, res) =>{ //req请求对象，res响应对象
    // res.send('delete user')
    try{
        //拿到对应id
        const roleID = Number.parseInt(req.params.id)
        const db = await getDb()
        const index = db.roles.findIndex(role => role.id === roleID)//查找索引
        if(index === -1){//roles中找不到相应的删除数据
            return res.status(404).end()
        }
        db.roles.splice(index, 1)//只删除一个
        await saveDb(db)
        res.status(204).end()
    }catch(err){
        res.status(500).json({
            error: err.message
        })
    }
})

app.listen(3000, ()=>{
    console.log('server running at http://localhost:3000/')
})

