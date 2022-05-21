import React, {useEffect, useState} from 'react';
import Image from "../../entities/image";
import Pagination from "../../UI/Pagination/Pagination";
import ImagesService from "../../../services/ImagesService";
import {useNavigate} from "react-router";
import SearchPanel from "../../UI/SearchPanel/SearchPanel";
import ReactLoading from 'react-loading'


const ImageList = () => {
    const pageSize = 10;

    const [images, setImages] = useState<Image[]>([]);
    const [maxPages, setMaxPages] = useState(0);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        ImagesService.getImagesPagedAsync({pageSize, pageNumber: 1}).then(x => {
            setImages(x.images);
            setMaxPages(Math.ceil(x.totalCount / pageSize));
            setLoading(false);
        }).catch(_ => setLoading(false))
    }, [])

    const downloadPageNumber = (pageNumber: number) => {
        setLoading(true);
        ImagesService.getImagesPagedAsync({pageSize, pageNumber}).then(x => {
            setImages(x.images);
            setLoading(false);
        }).catch(_ => setLoading(false))
    }

    const nav = useNavigate();
    const toEditImagePage = (id: number) => nav(`/images/${id}`);

    const onSearch = (q: string) => {
        const id = Number(q);
        if (!Number.isInteger(id)) {
            alert('Id must be an integer');
            return;
        }
        toEditImagePage(id);
    }

    return (
        <div className={'container mt-5'}>
            {loading
                ? <><ReactLoading className={'mx-auto'} type={'spinningBubbles'} color={'black'} height='200px' width='200px' /></>
                : <>
            <SearchPanel onSearch={onSearch} placeholder={'Enter id of image'}/>
            <div className='mt-5 mx-auto'>
                <table className={'usersTable table table-borderless table-light'}>
                    <thead>
                    <th className='firstRow usersRow'>
                        <td className='Cell w-10'>ID</td>
                        <td className='Cell w-35'>Name</td>
                        <td className='Cell w-10'>OwnerID</td>
                        <td className='Cell'>Filename</td>
                        <td className='Cell'>Upload time</td>
                    </th>
                    </thead>
                    <tbody className='mx-auto mt-5 table-hover'>
                    {images.map(i =>
                        <tr onClick={() => toEditImagePage(i.id)} className='usersRow'>
                            <td className='Cell w-10'>{i.id}</td>
                            <td className='Cell w-35'>
                                <div className={'bigtext'}> {i.name}</div>
                            </td>
                            <td className='Cell w-10'>{i.ownerId}</td>
                            <td className='Cell'>{i.filename}</td>
                            <td className='Cell'>{new Date(i.uploadDate).toLocaleString('ru')}</td>
                        </tr>
                    )}
                    </tbody>
                </table>
            </div>
                </>
            }
            <footer className={'footer fixed-bottom d-flex mb-0 justify-content-center'}>
                <Pagination totalPagesCount={maxPages} onPageChange={downloadPageNumber}/>
            </footer>
        </div>
    );
};

// @ts-ignore
export default ImageList;