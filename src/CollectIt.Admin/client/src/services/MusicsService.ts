import authorizedFetch from "./AuthorizedFetch";
import {serverAddress as server} from "../constants";
import NotFoundError from "../utils/NotFoundError";
import Music from "../components/entities/music";

const baseApiPath = `${server}/api/v1/musics`;

export default class MusicsService {
    private static readonly fetch = authorizedFetch();

    static async getMusicsPagedAsync({pageSize, pageNumber}: {pageSize: number, pageNumber: number})
        : Promise<Music[]> {
        if (pageSize < 1 || pageNumber < 1) {
            throw new Error('Page size and page number must be positive');
        }
        const response = await MusicsService.fetch(`${baseApiPath}?page_size=${pageSize}&page_number=${pageNumber}`, {
            method: 'GET'
        });
        if (!response.ok) {
            throw new Error('Could not get images from server');
        }
        return await response.json();
    }

    static async getMusicByIdAsync(id: number): Promise<Music> {
        const response = await MusicsService.fetch(`${baseApiPath}/${id}`, {
            method: 'GET'
        });
        try {
            if (!response.ok) {
                if (response.status === 404) {
                    throw new NotFoundError('Music with specified id not found');
                }
                throw new Error('Could not get music from server')
            }
            return await response.json();

        } catch (e: any) {
            throw new Error(e.message);
        }
    }


    static async changeMusicNameAsync(id: number, name: string) {
        if (!name) {
            throw new Error('No name provided');
        }

        const response = await MusicsService.fetch(`${baseApiPath}/${id}/name`, {
            method: 'POST',
            body: JSON.stringify({
                name: name
            }),
            headers: {
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) {
            console.error(`Could not change music name. Server status: ${response.status}`);
            throw new Error('Could not change music name');
        }
    }

    static async changeMusicTagsAsync(id: number, tags: string[]) {
        if (!tags) {
            throw new Error('Tags can not be null or undefined');
        }

        const response = await MusicsService.fetch(`${baseApiPath}/${id}/tags`, {
            method: 'POST',
            body: JSON.stringify({
                tags: tags
            }),
            headers: {
                'Content-Type': 'application/json'
            }
        });
        if (!response.ok) {
            console.error(`Could not change music tags. Server status: ${response.status}`);
            throw new Error('Could not change tags');
        }
    }
}