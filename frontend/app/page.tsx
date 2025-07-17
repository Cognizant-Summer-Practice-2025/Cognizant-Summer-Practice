'use client'
import React from 'react'
import { getServerSession } from 'next-auth'
import { authOptions } from './api/auth/[...nextauth]/route'
import Header  from '@/components/header'

const page = async () => {
  const session = await getServerSession(authOptions)
  console.log(session);

  return (
    <>
      <Header />
      <div>page</div>
    </>
  )
}

export default page